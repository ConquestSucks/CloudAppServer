using CloudAppServer.Application.Authentication.Interfaces;
using CloudAppServer.Application.Exceptions;
using CloudAppServer.Domain.Entities;
using CloudAppServer.Domain.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace CloudAppServer.Infrastructure.Services;

public class AuthenticationService(
    IUserLoginRequestRepository userLoginRequestRepository,
    ITelegramBotClient telegramBotClient,
    IAuthenticationSessionStore authenticationSessionStore) : IAuthenticationService
{
    public async Task<bool> TrySendLoginRequestAndWaitAsync(User user, TimeSpan timeout)
    {
        var sessionCreated = authenticationSessionStore.TryCreateAuthorizationSession(user.Name);
        if (!sessionCreated)
            throw new InternalServerErrorException("Не удалось создать сессию для аутентификации");
        
        var request = new UserLoginRequest
        {
            Name = user.Name,
            UserId = user.Id
        };

        await userLoginRequestRepository.AddAsync(request);
        
        var buttons = new[]
        {
            InlineKeyboardButton.WithCallbackData("Разрешить", $"login_{request.Id}_approve"),
            InlineKeyboardButton.WithCallbackData("Запретить", $"login_{request.Id}_deny")
        };
        
        var message = await telegramBotClient.SendMessage(
            chatId: user.TelegramChatId,
            text: "Мы получили запрос на вход.\n\nЧтобы принять запрос, нажмите на кнопку \"Разрешить\" ниже.",
            replyMarkup: buttons
        );
        
        var authenticationUserResponse = await authenticationSessionStore.WaitForUserResponse(user.Name, timeout);
        if (authenticationUserResponse.IsAuthenticationApproved) 
            return true;

        if (authenticationUserResponse is { IsTimeout: false, IsAuthenticationApproved: false })
            throw new ForbiddenException("Вход запрещен");
        
        await telegramBotClient.EditMessageText(
            chatId: user.TelegramChatId,
            message.Id,
            text: "Мы получили запрос на вход.\n\nЧтобы принять запрос, нажмите на кнопку \"Разрешить\" ниже.\n\n(Время ожидания ответа на запрос вышло)");
        
        request.Deny();
        await userLoginRequestRepository.UpdateAsync(request);
            
        throw new AuthenticationTimeoutException("Не поступило ответа на запрос аутентификации");
    }
}