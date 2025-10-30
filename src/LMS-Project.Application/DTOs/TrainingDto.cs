namespace LMS_Project.Application.DTOs;

public class TrainingDto
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalDurationSeconds { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? VideoIntroPath { get; set; }
    public int TotalSubTopics { get; set; }
    public int CompletedSubTopics { get; set; }
    public decimal CompletionPercentage { get; set; }
    public List<SubTopicDto> SubTopics { get; set; } = new();

    // ⭐ Eğitim kilitleme bilgisi (Training Request System)
    public bool IsLocked { get; set; }
    public bool HasPendingRequest { get; set; }
    public string? LockReason { get; set; }
}

public class TrainingListDto
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalDurationSeconds { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public string? ThumbnailPath { get; set; }
    public int TotalSubTopics { get; set; }
    public decimal CompletionPercentage { get; set; }
}

public class MyTrainingDto
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalDurationSeconds { get; set; }
    public int OrderIndex { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? VideoIntroPath { get; set; }

    // Progress bilgileri
    public int TotalSubTopics { get; set; }
    public int CompletedSubTopics { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? StartedDate { get; set; }

    // Kullanıcının toplam harcadığı süre
    public int TotalStudyTimeSeconds { get; set; }

    // Sıradaki erişilebilir SubTopic
    public Guid? NextAvailableSubTopicId { get; set; }
    public string? NextAvailableSubTopicName { get; set; }
}
