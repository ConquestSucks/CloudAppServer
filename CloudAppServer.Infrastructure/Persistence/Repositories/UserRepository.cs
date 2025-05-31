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
    
    public async Task<decimal> GetUserFreeDiskSpace(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user is null)
            return 0;

        var diskSpaceOccupied = await GetUserDiskSpaceOccupied(userId);
        var freeDiskSpace = user.DiskSpace - diskSpaceOccupied;
        
        return freeDiskSpace < 0 ? 0 : freeDiskSpace;
    }

    public async Task<decimal> GetUserDiskSpaceOccupied(Guid userId)
    {
        return await DbContext.CloudFiles
            .Where(f => f.UserId == userId)
            .Select(f => f.Size)
            .SumAsync();
    }

    public async Task<bool> IsUserExists(string userLogin)
    {
        return await DbContext.Users.AnyAsync(u => u.Name == userLogin);
    }
}