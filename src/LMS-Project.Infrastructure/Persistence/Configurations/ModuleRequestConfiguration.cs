using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Project.Infrastructure.Persistence.Configurations;

public class ModuleRequestConfiguration : IEntityTypeConfiguration<ModuleRequest>
{
    public void Configure(EntityTypeBuilder<ModuleRequest> builder)
    {
        builder.ToTable("ModuleRequests");

        builder.HasKey(mr => mr.Id);

        builder.Property(mr => mr.RequestReason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(mr => mr.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(mr => mr.ReviewNote)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(mr => mr.User)
            .WithMany()
            .HasForeignKey(mr => mr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Module)
            .WithMany()
            .HasForeignKey(mr => mr.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Reviewer)
            .WithMany()
            .HasForeignKey(mr => mr.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(mr => mr.UserId);
        builder.HasIndex(mr => mr.ModuleId);
        builder.HasIndex(mr => mr.Status);
        builder.HasIndex(mr => new { mr.UserId, mr.ModuleId, mr.Status });
    }
}
