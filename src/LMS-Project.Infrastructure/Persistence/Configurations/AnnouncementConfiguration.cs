using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Project.Infrastructure.Persistence.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Content)
            .IsRequired();

        builder.Property(a => a.Priority)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.TargetRole)
            .HasMaxLength(50);

        builder.Property(a => a.ImagePath)
            .HasMaxLength(500);

        builder.Property(a => a.PublishDate)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(a => a.PublishDate);
        builder.HasIndex(a => a.Priority);
        builder.HasIndex(a => a.IsActive);
    }
}
