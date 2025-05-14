using CloudAppServer.Domain.Entities;

namespace CloudAppServer.Application.Interfaces;

public interface ITelegramService
{
    Task SendLoginRequestAsync(User user);
}