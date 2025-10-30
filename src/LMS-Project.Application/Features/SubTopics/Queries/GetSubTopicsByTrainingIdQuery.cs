using LMS_Project.Application.DTOs;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.SubTopics.Queries;

/// <summary>
/// Bir eğitime ait tüm alt başlıkları getirir (Admin/Moderator için)
/// </summary>
public record GetSubTopicsByTrainingIdQuery(Guid TrainingId) : IRequest<Result<List<SubTopicListDto>>>;

public class SubTopicListDto
{
    public Guid Id { get; set; }
    public Guid TrainingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinimumDurationSeconds { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public bool IsMandatory { get; set; }
    public string? ThumbnailPath { get; set; }
    public string ZipFilePath { get; set; } = string.Empty;
    public string HtmlFilePath { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class GetSubTopicsByTrainingIdQueryHandler : IRequestHandler<GetSubTopicsByTrainingIdQuery, Result<List<SubTopicListDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSubTopicsByTrainingIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<SubTopicListDto>>> Handle(GetSubTopicsByTrainingIdQuery request, CancellationToken cancellationToken)
    {
        // Önce eğitim var mı kontrol et
        var training = await _unitOfWork.Trainings.GetByIdAsync(request.TrainingId, cancellationToken);
        if (training == null)
        {
            return Result<List<SubTopicListDto>>.Failure("Eğitim bulunamadı.");
        }

        // Alt başlıkları getir
        var subTopics = await _unitOfWork.SubTopics.GetSubTopicsByTrainingIdAsync(request.TrainingId, cancellationToken);

        if (!subTopics.Any())
        {
            return Result<List<SubTopicListDto>>.Failure("Bu eğitime ait hiç alt başlık bulunamadı.");
        }

        var subTopicDtos = subTopics.OrderBy(st => st.OrderIndex).Select(st => new SubTopicListDto
        {
            Id = st.Id,
            TrainingId = st.TrainingId,
            Name = st.Name,
            Description = st.Description,
            MinimumDurationSeconds = st.MinimumDurationSeconds,
            OrderIndex = st.OrderIndex,
            IsActive = st.IsActive,
            IsMandatory = st.IsMandatory,
            ThumbnailPath = st.ThumbnailPath,
            ZipFilePath = st.ZipFilePath,
            HtmlFilePath = st.HtmlFilePath,
            CreatedDate = st.CreatedDate
        }).ToList();

        var message = $"{training.Name} eğitimine ait {subTopicDtos.Count} alt başlık bulundu.";
        return Result<List<SubTopicListDto>>.Success(subTopicDtos, message);
    }
}
