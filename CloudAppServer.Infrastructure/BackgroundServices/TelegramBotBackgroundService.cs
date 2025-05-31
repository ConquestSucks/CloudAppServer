using System.Text;
using CloudAppServer.Application.Authentication.Interfaces;
using CloudAppServer.Domain.Entities;
using CloudAppServer.Domain.Enums;
using CloudAppServer.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = CloudAppServer.Domain.Entities.User;

namespace CloudAppServer.Infrastructure.BackgroundServices;

public class TelegramBotBackgroundService(
    ITelegramBotClient telegramBotClient, 
    IServiceScopeFactory serviceScopeFactory, 
    ILogger<TelegramBotBackgroundService> logger,
    IAuthenticationSessionStore authenticationSessionStore) : BackgroundService
{
    private static readonly Random Random = new();

    private const string Letters = "abcdefghijklmnopqrstuvwxyz";

    private static readonly string[] FirstWords =
    [
        "Alpha",
        "Bravo",
        "Charlie",
        "Delta",
        "Echo",
        "Foxtrot",
        "Golf",
        "Hotel",
        "India",
        "Juliet"
    ];
    
    private static readonly string[] SecondWords =
    [
        "Kilo",
        "Lima",
        "Mike",
        "November",
        "Oscar",
        "Papa",
        "Quebec",
        "Romeo",
        "Sierra",
        "Tango"
    ];

    public static string GenerateUsername()
    {
        var index1 = Random.Next(0, FirstWords.Length);
        var index2 = Random.Next(0, SecondWords.Length);

        var word1 = FirstWords[index1];
        var word2 = SecondWords[index2];
        
        var number = Random.Next(10000, 100000);
        
        return word1 + word2 + number;
    }
    
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

    private async Task<bool> CheckIsUserAlreadyRegistered(long chatId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        return await userRepository.DoesUserExistByTelegramChatIdAsync(chatId);
    }

    private static InlineKeyboardMarkup GetKeyboard(bool registered)
    {
        var text = registered ? "👤 Мой логин" : "🪪 Зарегистрироваться";
        var callbackData = registered ? "mylogin_callback" : "register_callback";
        return new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(text, callbackData));
    }

    private async Task InvokeRegisterCommand(ITelegramBotClient bot, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = GetKeyboard(true);
        if (await CheckIsUserAlreadyRegistered(chatId))
        {
            await bot.SendMessage(chatId, 
                "Вы уже зарегистрированы. Воспользуйтесь кнопкой \"Мой логин\" или командой /mylogin для входа на сайт.",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
                            
            return;
        }
                
        var newLogin = GenerateLogin();
        var username = GenerateUsername();
        var user = new User
        {
            DisplayName = username,
            Name = newLogin,
            TelegramChatId = chatId,
            DiskSpace = 20 * 1024 * 1024 * 1024L
        };
                        
        using var scope = serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                
        await userRepository.AddAsync(user);
                        
        await bot.SendMessage(user.TelegramChatId, 
            "Вы успешно зарегистрировались. Используйте кнопку \"Мой логин\" или команду /mylogin для входа на сайт.",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
    
    private async Task InvokeMyLoginCommand(ITelegramBotClient bot, long chatId, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        
        var user = await userRepository.FindUserByTelegramChatIdAsync(chatId);
        if (user is null)
        {
            await bot.SendMessage(chatId, 
                "Похоже вы тут впервые, используйте кнопку \"Зарегистрироваться\" или команду /register для регистрации на сайте.",
                replyMarkup: GetKeyboard(false),
                cancellationToken: cancellationToken);
                        
            return;
        }

        await bot.SendMessage(user.TelegramChatId,
            $"👤 Ваш логин для входа на сайт: `{user.Name}`",
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCopyText("Скопировать логин", 
                new CopyTextButton
                {
                    Text = user.Name
                })), 
            cancellationToken: cancellationToken);
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
                        var chatId = update.Message.Chat.Id;
                        var isUserAlreadyRegistered = await CheckIsUserAlreadyRegistered(chatId);

                        var text = isUserAlreadyRegistered
                            ? "У вас есть зарегистрированный аккаунт. Воспользуйтесь кнопкой \"Мой логин\" или командой /mylogin для входа на сайт."
                            : "Похоже вы тут впервые, используйте кнопку \"Зарегистрироваться\" или команду /register для регистрации на сайте.";
                        await bot.SendMessage(
                            chatId: chatId,
                            text: text,
                            replyMarkup: GetKeyboard(isUserAlreadyRegistered),
                            cancellationToken: cancellationToken);
                        
                        return;
                    }
                    case "/register":
                    {
                        await InvokeRegisterCommand(bot, update.Message.Chat.Id, cancellationToken);
                        
                        return;
                    }
                    case "/mylogin":
                    {
                        await InvokeMyLoginCommand(bot, update.Message.Chat.Id, cancellationToken);
                        
                        return;
                    }
                }

                break;
            }
            case UpdateType.CallbackQuery when string.IsNullOrWhiteSpace(update.CallbackQuery?.Data):
                return;
            case UpdateType.CallbackQuery when !string.IsNullOrWhiteSpace(update.CallbackQuery.Data) 
                                               && update.CallbackQuery.Message is not null:
            {
                var buttons = new List<string>
                {
                    "mylogin_callback",
                    "register_callback"
                };
                if (buttons.Contains(update.CallbackQuery.Data))
                {
                    switch (update.CallbackQuery.Data)
                    {
                        case "mylogin_callback":
                            await InvokeMyLoginCommand(bot, update.CallbackQuery.Message.Chat.Id, cancellationToken);
                            break;
                        case "register_callback":
                            await InvokeRegisterCommand(bot, update.CallbackQuery.Message.Chat.Id, cancellationToken);
                            break;
                    }

                    return;
                }
                
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

                if (approve)
                {
                    request.Approve();
                    
                    authenticationSessionStore.ApproveUserAuthorization(user.Name);
                }
                else
                {
                    request.Deny();
                    
                    authenticationSessionStore.DenyUserAuthorization(user.Name);
                }

                await repository.UpdateAsync(request);
                
                var approveText = approve ? "разрешен" : "запрещен";
                var emoji = approve ? "✅" : "❌";
                await telegramBotClient.EditMessageText(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.Id,
                    text: $"Мы получили запрос на вход.\n\nЧтобы принять запрос, нажмите на кнопку \"Разрешить\" ниже.\n\n{emoji} (Вход {approveText})", 
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