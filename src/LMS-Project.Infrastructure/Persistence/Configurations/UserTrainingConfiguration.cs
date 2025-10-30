using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Project.Infrastructure.Persistence.Configurations;

public class UserTrainingConfiguration : IEntityTypeConfiguration<UserTraining>
{
    public void Configure(EntityTypeBuilder<UserTraining> builder)
    {
        builder.HasKey(ut => ut.Id);

        // Decimal precision ayarı
        builder.Property(ut => ut.CompletionPercentage)
            .HasPrecision(5, 2); // 0.00 - 100.00

        builder.HasIndex(ut => new { ut.UserId, ut.TrainingId })
            .IsUnique();

        builder.HasIndex(ut => ut.UserId);
        builder.HasIndex(ut => ut.TrainingId);
        builder.HasIndex(ut => ut.IsCompleted);
    }
}
