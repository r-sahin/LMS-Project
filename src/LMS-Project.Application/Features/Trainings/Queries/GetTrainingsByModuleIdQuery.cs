using LMS_Project.Application.DTOs;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Trainings.Queries;

/// <summary>
/// Bir modüle ait tüm eğitimleri getirir (Admin/Moderator için)
/// </summary>
public record GetTrainingsByModuleIdQuery(Guid ModuleId, bool IncludeUnpublished = true) : IRequest<Result<List<TrainingListDto>>>;

public class TrainingListDto
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalDurationSeconds { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? PublishedBy { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? VideoIntroPath { get; set; }
    public int TotalSubTopics { get; set; }
    public int TotalSubTopicDuration { get; set; }
    public bool CanBePublished { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class GetTrainingsByModuleIdQueryHandler : IRequestHandler<GetTrainingsByModuleIdQuery, Result<List<TrainingListDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTrainingsByModuleIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<TrainingListDto>>> Handle(GetTrainingsByModuleIdQuery request, CancellationToken cancellationToken)
    {
        // Önce modül var mı kontrol et
        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module == null)
        {
            return Result<List<TrainingListDto>>.Failure("Modül bulunamadı.");
        }

        // Eğitimleri getir
        var trainings = await _unitOfWork.Trainings.GetTrainingsByModuleIdAsync(request.ModuleId, cancellationToken);

        // IncludeUnpublished false ise sadece yayınlanmışları filtrele
        if (!request.IncludeUnpublished)
        {
            trainings = trainings.Where(t => t.IsPublished).ToList();
        }

        if (!trainings.Any())
        {
            var message = request.IncludeUnpublished
                ? "Bu modüle ait hiç eğitim bulunamadı."
                : "Bu modüle ait yayınlanmış hiç eğitim bulunamadı.";
            return Result<List<TrainingListDto>>.Failure(message);
        }

        var trainingDtos = new List<TrainingListDto>();

        foreach (var training in trainings.OrderBy(t => t.OrderIndex))
        {
            // Alt başlıkları getir
            var subTopics = await _unitOfWork.SubTopics.GetSubTopicsByTrainingIdAsync(training.Id, cancellationToken);
            var totalSubTopicDuration = subTopics.Sum(st => st.MinimumDurationSeconds);

            // Yayınlanabilir mi kontrolü
            var canBePublished = subTopics.Any() && totalSubTopicDuration == training.TotalDurationSeconds;

            trainingDtos.Add(new TrainingListDto
            {
                Id = training.Id,
                ModuleId = training.ModuleId,
                Name = training.Name,
                Description = training.Description,
                TotalDurationSeconds = training.TotalDurationSeconds,
                OrderIndex = training.OrderIndex,
                IsActive = training.IsActive,
                IsPublished = training.IsPublished,
                PublishedDate = training.PublishedDate,
                PublishedBy = training.PublishedBy,
                ThumbnailPath = training.ThumbnailPath,
                VideoIntroPath = training.VideoIntroPath,
                TotalSubTopics = subTopics.Count(),
                TotalSubTopicDuration = totalSubTopicDuration,
                CanBePublished = canBePublished,
                CreatedDate = training.CreatedDate
            });
        }

        var successMessage = $"{module.Name} modülüne ait {trainingDtos.Count} eğitim bulundu. ({trainingDtos.Count(t => t.IsPublished)} yayınlanmış)";
        return Result<List<TrainingListDto>>.Success(trainingDtos, successMessage);
    }
}
