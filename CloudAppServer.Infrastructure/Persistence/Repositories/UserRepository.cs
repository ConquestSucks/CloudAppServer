using CloudAppServer.Domain.Entities;
using CloudAppServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudAppServer.Infrastructure.Persistence.Repositories;

public class UserRepository(CloudAppDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
    public Task<User?> FindUserByNameAsync(string name)
    {
        return DbContext.Users.FirstOrDefaultAsync(u => u.Name == name);
    }

    public Task<User?> FindUserByTelegramChatIdAsync(long chatId)
    {
        return DbContext.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);
    }

    public Task<bool> DoesUserExistByTelegramChatIdAsync(long chatId)
    {
        return DbContext.Users.AnyAsync(u => u.TelegramChatId == chatId);
    }
}