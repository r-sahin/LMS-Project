using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Project.Infrastructure.Persistence.Configurations;

public class TrainingConfiguration : IEntityTypeConfiguration<Training>
{
    public void Configure(EntityTypeBuilder<Training> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.ThumbnailPath)
            .HasMaxLength(500);

        builder.Property(t => t.VideoIntroPath)
            .HasMaxLength(500);

        builder.HasMany(t => t.SubTopics)
            .WithOne(st => st.Training)
            .HasForeignKey(st => st.TrainingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.ModuleId);
        builder.HasIndex(t => t.OrderIndex);
        builder.HasIndex(t => t.IsActive);
    }
}
