using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Project.Infrastructure.Persistence.Configurations;

public class SubTopicConfiguration : IEntityTypeConfiguration<SubTopic>
{
    public void Configure(EntityTypeBuilder<SubTopic> builder)
    {
        builder.HasKey(st => st.Id);

        builder.Property(st => st.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(st => st.Description)
            .HasMaxLength(1000);

        builder.Property(st => st.ZipFilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(st => st.HtmlFilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(st => st.ThumbnailPath)
            .HasMaxLength(500);

        builder.HasMany(st => st.UserProgresses)
            .WithOne(up => up.SubTopic)
            .HasForeignKey(up => up.SubTopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(st => st.TrainingId);
        builder.HasIndex(st => st.OrderIndex);
        builder.HasIndex(st => st.IsActive);
    }
}
