namespace LMS_Project.Application.DTOs;

public class UserProgressDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SubTopicId { get; set; }
    public string SubTopicName { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int MinimumDurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime LastAccessedDate { get; set; }
    public int AccessCount { get; set; }
    public bool IsLocked { get; set; }
    public decimal CompletionPercentage { get; set; }
}

public class ProgressUpdateDto
{
    public Guid SubTopicId { get; set; }
    public int DurationSeconds { get; set; }
}

public class ProgressSummaryDto
{
    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public int TotalTrainings { get; set; }
    public int CompletedTrainings { get; set; }
    public int TotalSubTopics { get; set; }
    public int CompletedSubTopics { get; set; }
    public decimal ModuleCompletionPercentage { get; set; }
    public List<TrainingProgressDto> Trainings { get; set; } = new();
}

public class TrainingProgressDto
{
    public Guid TrainingId { get; set; }
    public string TrainingName { get; set; } = string.Empty;
    public int TotalSubTopics { get; set; }
    public int CompletedSubTopics { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsCompleted { get; set; }
}
