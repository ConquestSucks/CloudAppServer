using System.Text;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Entities;
using CloudAppServer.Domain.Enums;
using CloudAppServer.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = CloudAppServer.Domain.Entities.User;

namespace CloudAppServer.Infrastructure.BackgroundServices;

public class TelegramBotBackgroundService(
    ITelegramBotClient telegramBotClient, 
    IServiceScopeFactory serviceScopeFactory, 
    ILogger<TelegramBotBackgroundService> logger) : BackgroundService
{
    private static readonly Random Random = new();

    private const string Letters = "abcdefghijklmnopqrstuvwxyz";

    private static string GenerateLogin()
    {
        var sb = new StringBuilder(17);
        for (var i = 0; i < 12; i++)
        {
            sb.Append(Random.Next(0, 10));
        }
        
        sb.Append('-');
        
        for (var i = 0; i < 4; i++)
        {
            var index = Random.Next(Letters.Length);
            sb.Append(Letters[index]);
        }

        return sb.ToString();
    }

    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        telegramBotClient.StartReceiving(OnUpdate, OnError, cancellationToken: cancellationToken);
        
        await Task.Delay(-1, cancellationToken);
    }

    private async Task OnUpdate(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.Message when update.Message is null:
                return;
            case UpdateType.Message:
            {
                var message = update.Message.Text;
                if (string.IsNullOrWhiteSpace(message))
                    return;

                switch (message)
                {
                    case "/start":
                    {
                        using var scope = serviceScopeFactory.CreateScope();
                        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                        if (await userRepository.DoesUserExistByTelegramChatIdAsync(update.Message.Chat.Id))
                            return;
                
                        var newLogin = GenerateLogin();
                        var user = new User
                        {
                            Name = newLogin,
                            TelegramChatId = update.Message.Chat.Id,
                            FreeDiskSpace = 10,
                            DiskSpaceLeft = 10
                        };
                
                        await userRepository.AddAsync(user);

                        await bot.SendMessage(user.TelegramChatId, $"Ваш логин: {newLogin}", cancellationToken: cancellationToken);
                        return;
                    }
                    case "/login":
                    {
                        using var scope = serviceScopeFactory.CreateScope();
                        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        var user = await userRepository.FindUserByTelegramChatIdAsync(update.Message.Chat.Id);
                        if (user is null)
                        {
                            await bot.SendMessage(update.Message.Chat, "Используйте команду /start для регистрации", cancellationToken: cancellationToken);
                        
                            return;
                        }
                    
                        await bot.SendMessage(user.TelegramChatId, $"Ваш логин: {user.Name}", cancellationToken: cancellationToken);
                        return;
                    }
                }

                break;
            }
            case UpdateType.CallbackQuery when string.IsNullOrWhiteSpace(update.CallbackQuery?.Data):
                return;
            case UpdateType.CallbackQuery when update.CallbackQuery.Message is not null:
            {
                using var scope = serviceScopeFactory.CreateScope();
                
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var user = await userRepository.FindUserByTelegramChatIdAsync(update.CallbackQuery.Message.Chat.Id);
                if (user is null)
                    return;
                
                var data = update.CallbackQuery.Data; // "login_{requestId}_approve" или "..._deny"
                var parts = data.Split('_');
                var requestId = Guid.Parse(parts[1]);
                var approve = parts[2] == "approve";
                
                var repository = scope.ServiceProvider.GetRequiredService<IRepository<UserLoginRequest>>();
                var request = await repository.GetByIdAsync(requestId);
                if (request is null || request.LoginRequestStatus != LoginRequestStatus.None)
                    return;
        
                var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

                if (approve)
                {
                    request.Approve();
                    
                    authorizationService.ApproveUserAuthorization(user.Name);
                }
                else
                {
                    request.Deny();
                    
                    authorizationService.DenyUserAuthorization(user.Name);
                }

                await repository.UpdateAsync(request);
                
                var approveText = approve ? "разрешен" : "запрещен";
                await telegramBotClient.EditMessageText(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.Id,
                    text: $"Мы получили запрос на вход.\n\nЧтобы принять запрос, нажмите на кнопку \"Разрешить\" ниже.\n\n(Вход {approveText})", 
                    cancellationToken: cancellationToken);
                return;
            }
        }
    }

    private Task OnError(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An error occured in telegram bot");
        
        return Task.CompletedTask;
    }
}