using LMS_Project.Domain.Common;

namespace LMS_Project.Domain.Entities;

public class ModuleRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ModuleId { get; set; }
    public string RequestReason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; 
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewNote { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual Module Module { get; set; } = null!;
    public virtual User? Reviewer { get; set; }
}
