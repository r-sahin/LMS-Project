using LMS_Project.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS_Project.Domain.Entities;
public class Certificate : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? ModuleId { get; set; }
    public Guid? TrainingId { get; set; }
    public string CertificateType { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public string PdfFilePath { get; set; } = string.Empty;
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedDate { get; set; }
    public string? RevokedReason { get; set; }
    public string? VerificationCode { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Module? Module { get; set; }
    public virtual Training? Training { get; set; }
}

