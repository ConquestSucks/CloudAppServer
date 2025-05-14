using CloudAppServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudAppServer.Infrastructure.Persistence.Configurations;

public class CloudFileConfiguration : IEntityTypeConfiguration<CloudFile>
{
    public void Configure(EntityTypeBuilder<CloudFile> builder)
    {
        builder.OwnsOne(cf => cf.PublicUrl, b =>
        {
            b.Property(pu => pu.Value)
                .HasColumnName("PublicUrl");
        });
    }
}