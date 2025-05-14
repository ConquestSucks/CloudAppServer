using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Entities;
using CloudAppServer.Domain.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace CloudAppServer.Infrastructure.Services;

public class TelegramService(ITelegramBotClient telegramBotClient, IRepository<UserLoginRequest> userLoginRequestRepository) : ITelegramService
{
    public async Task SendLoginRequestAsync(User user)
    {
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
        
        await telegramBotClient.SendMessage(
            chatId: user.TelegramChatId,
            text: "Мы получили запрос на вход.\n\nЧтобы принять запрос, нажмите на кнопку \"Разрешить\" ниже.",
            replyMarkup: buttons
        );
    }
}