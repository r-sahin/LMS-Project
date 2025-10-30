using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Project.Infrastructure.Persistence.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CertificateType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.CertificateNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.PdfFilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.VerificationCode)
            .HasMaxLength(50);

        builder.HasIndex(c => c.CertificateNumber).IsUnique();
        builder.HasIndex(c => c.VerificationCode).IsUnique();
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.IsRevoked);
    }
}
