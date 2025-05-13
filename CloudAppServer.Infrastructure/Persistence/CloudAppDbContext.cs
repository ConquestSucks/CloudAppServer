using CloudApp.Domain.Entities;
using CloudApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CloudApp.Infrastructure.Persistence;

public class CloudAppDbContext(DbContextOptions<CloudAppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<CloudFile> CloudFiles { get; set; }
    
    public DbSet<CloudFolder> CloudFolders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}