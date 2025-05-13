using CloudApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudApp.Infrastructure.Persistence.Configurations;

public class CloudFolderConfiguration : IEntityTypeConfiguration<CloudFolder>
{
    public void Configure(EntityTypeBuilder<CloudFolder> builder)
    {
        builder.OwnsOne(cf => cf.PublicUrl, b =>
        {
            b.Property(pu => pu.Value)
                .HasColumnName("PublicUrl");
        });
    }
}