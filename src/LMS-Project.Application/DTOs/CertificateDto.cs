namespace LMS_Project.Application.DTOs;

public class CertificateDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public Guid? ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public Guid? TrainingId { get; set; }
    public string? TrainingName { get; set; }
    public string CertificateType { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public string PdfFilePath { get; set; } = string.Empty;
    public bool IsRevoked { get; set; }
    public DateTime? RevokedDate { get; set; }
    public string? RevokedReason { get; set; }
    public string? VerificationCode { get; set; }
}

public class CertificateListDto
{
    public Guid Id { get; set; }
    public string CertificateType { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public bool IsRevoked { get; set; }
}
