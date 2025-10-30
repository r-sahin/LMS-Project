namespace LMS_Project.Application.DTOs;

public class AnnouncementDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public string Priority { get; set; } = "Normal";
    public string? TargetRole { get; set; }
    public string? ImagePath { get; set; }
    public DateTime CreatedDate { get; set; }
}
