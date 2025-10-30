namespace LMS_Project.Application.DTOs;

public class SubTopicDto
{
    public Guid Id { get; set; }
    public Guid TrainingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinimumDurationSeconds { get; set; }
    public string ZipFilePath { get; set; } = string.Empty;
    public string HtmlFilePath { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public bool IsMandatory { get; set; }
    public string? ThumbnailPath { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLocked { get; set; }
    public int CurrentDurationSeconds { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? LastAccessedDate { get; set; }
}

public class SubTopicListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinimumDurationSeconds { get; set; }
    public int OrderIndex { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLocked { get; set; }
    public string? ThumbnailPath { get; set; }
}
