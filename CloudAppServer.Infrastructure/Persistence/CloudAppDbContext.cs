using CloudAppServer.Domain.Entities;
using CloudAppServer.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CloudAppServer.Infrastructure.Persistence;

public class CloudAppDbContext(DbContextOptions<CloudAppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<UserLoginRequest> UserLoginRequests { get; set; }
    
    public DbSet<CloudFile> CloudFiles { get; set; }
    
    public DbSet<CloudFolder> CloudFolders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}