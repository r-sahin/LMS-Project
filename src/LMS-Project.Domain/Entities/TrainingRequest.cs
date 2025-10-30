using LMS_Project.Domain.Common;

namespace LMS_Project.Domain.Entities;

public class TrainingRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid TrainingId { get; set; }
    public string RequestReason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewNote { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Training Training { get; set; } = null!;
    public virtual User? Reviewer { get; set; }
}
