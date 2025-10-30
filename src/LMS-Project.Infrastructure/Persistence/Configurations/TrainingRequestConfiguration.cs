using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Project.Infrastructure.Persistence.Configurations;

public class TrainingRequestConfiguration : IEntityTypeConfiguration<TrainingRequest>
{
    public void Configure(EntityTypeBuilder<TrainingRequest> builder)
    {
        builder.HasKey(tr => tr.Id);

        builder.Property(tr => tr.RequestReason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(tr => tr.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tr => tr.ReviewNote)
            .HasMaxLength(500);

        // ⭐ User ilişkisi (Talep eden kullanıcı)
        builder.HasOne(tr => tr.User)
            .WithMany()
            .HasForeignKey(tr => tr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ⭐ Reviewer ilişkisi (Onaylayan/Reddeden moderator)
        builder.HasOne(tr => tr.Reviewer)
            .WithMany()
            .HasForeignKey(tr => tr.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Training ilişkisi
        builder.HasOne(tr => tr.Training)
            .WithMany()
            .HasForeignKey(tr => tr.TrainingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index'ler
        builder.HasIndex(tr => tr.UserId);
        builder.HasIndex(tr => tr.TrainingId);
        builder.HasIndex(tr => tr.Status);
        builder.HasIndex(tr => new { tr.UserId, tr.TrainingId }); // Composite index
    }
}
