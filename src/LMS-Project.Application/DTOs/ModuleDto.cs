namespace LMS_Project.Application.DTOs;

public class ModuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public string? ImagePath { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public int TotalTrainings { get; set; }
    public int CompletedTrainings { get; set; }
    public decimal CompletionPercentage { get; set; }
    public List<TrainingDto> Trainings { get; set; } = new();
}

public class ModuleListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public string? ImagePath { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public int TotalTrainings { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsCompleted { get; set; }
}
