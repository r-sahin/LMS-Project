using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Project.Infrastructure.Persistence.Configurations;

public class UserModuleConfiguration : IEntityTypeConfiguration<UserModule>
{
    public void Configure(EntityTypeBuilder<UserModule> builder)
    {
        builder.HasKey(um => um.Id);

        // Decimal precision ayarı
        builder.Property(um => um.CompletionPercentage)
            .HasPrecision(5, 2); // 0.00 - 100.00

        builder.HasIndex(um => new { um.UserId, um.ModuleId })
            .IsUnique();

        builder.HasIndex(um => um.UserId);
        builder.HasIndex(um => um.ModuleId);
        builder.HasIndex(um => um.IsCompleted);
    }
}
