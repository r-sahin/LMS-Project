namespace LMS_Project.Application.DTOs;

public class AvailableModuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool HasPendingRequest { get; set; }
    public string? LastRequestStatus { get; set; }
    public DateTime? LastRequestDate { get; set; }
}
