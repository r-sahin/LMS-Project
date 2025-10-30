using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Project.Infrastructure.Persistence.Configurations;

public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.HasKey(up => up.Id);

        builder.HasIndex(up => new { up.UserId, up.SubTopicId })
            .IsUnique();

        builder.HasIndex(up => up.UserId);
        builder.HasIndex(up => up.SubTopicId);
        builder.HasIndex(up => up.IsCompleted);
        builder.HasIndex(up => up.LastAccessedDate);

        // Session tracking
        builder.Property(up => up.SessionId).HasMaxLength(255);
        builder.Property(up => up.IpAddress).HasMaxLength(50);
        builder.Property(up => up.DeviceInfo).HasMaxLength(500);
        builder.Property(up => up.IsSessionActive).HasDefaultValue(true);
    }
}
