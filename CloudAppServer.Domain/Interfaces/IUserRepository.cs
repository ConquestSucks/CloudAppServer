using CloudAppServer.Domain.Entities;

namespace CloudAppServer.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> FindUserByNameAsync(string name);
    
    Task<User?> FindUserByTelegramChatIdAsync(long chatId);
    
    Task<bool> DoesUserExistByTelegramChatIdAsync(long chatId);
}