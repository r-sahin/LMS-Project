using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Trainings.Queries;

public record GetTrainingDurationInfoQuery(Guid TrainingId) : IRequest<Result<TrainingDurationInfoDto>>;

public record TrainingDurationInfoDto(
    Guid TrainingId,
    string TrainingName,
    int TrainingTotalSeconds,
    int TrainingTotalMinutes,
    int UsedSeconds,
    int RemainingSeconds,
    int RemainingMinutes,
    int RemainingSecondsRemainder,
    decimal UsagePercentage,
    bool IsPublished,
    bool CanBePublished,
    string? PublishBlockReason,
    List<SubTopicDurationDto> SubTopics);

public record SubTopicDurationDto(
    Guid SubTopicId,
    string SubTopicName,
    int DurationSeconds,
    int DurationMinutes);

public class GetTrainingDurationInfoQueryHandler : IRequestHandler<GetTrainingDurationInfoQuery, Result<TrainingDurationInfoDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTrainingDurationInfoQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TrainingDurationInfoDto>> Handle(GetTrainingDurationInfoQuery request, CancellationToken cancellationToken)
    {
        var training = await _unitOfWork.Trainings.GetByIdAsync(request.TrainingId, cancellationToken);
        if (training == null)
        {
            return Result<TrainingDurationInfoDto>.Failure("Eğitim bulunamadı.");
        }

        var subTopics = await _unitOfWork.SubTopics.GetSubTopicsByTrainingIdAsync(request.TrainingId, cancellationToken);

        var usedSeconds = subTopics.Sum(st => st.MinimumDurationSeconds);
        var remainingSeconds = training.TotalDurationSeconds - usedSeconds;
        var remainingMinutes = remainingSeconds / 60;
        var remainingSecondsRemainder = remainingSeconds % 60;
        var usagePercentage = training.TotalDurationSeconds > 0
            ? Math.Round((decimal)usedSeconds / training.TotalDurationSeconds * 100, 2)
            : 0;

        // Yayınlanabilir mi kontrolü
        bool canBePublished = subTopics.Any() && usedSeconds == training.TotalDurationSeconds;
        string? publishBlockReason = null;

        if (!subTopics.Any())
        {
            publishBlockReason = "En az bir alt başlık eklemelisiniz.";
        }
        else if (usedSeconds != training.TotalDurationSeconds)
        {
            if (usedSeconds < training.TotalDurationSeconds)
            {
                publishBlockReason = $"Alt başlıkların toplam süresi eğitim süresinden {remainingMinutes}dk {remainingSecondsRemainder}sn eksik.";
            }
            else
            {
                var excess = usedSeconds - training.TotalDurationSeconds;
                var excessMin = excess / 60;
                var excessSec = excess % 60;
                publishBlockReason = $"Alt başlıkların toplam süresi eğitim süresinden {excessMin}dk {excessSec}sn fazla.";
            }
        }

        var subTopicDtos = subTopics.Select(st => new SubTopicDurationDto(
            st.Id,
            st.Name,
            st.MinimumDurationSeconds,
            st.MinimumDurationSeconds / 60)).ToList();

        var dto = new TrainingDurationInfoDto(
            training.Id,
            training.Name,
            training.TotalDurationSeconds,
            training.TotalDurationSeconds / 60,
            usedSeconds,
            remainingSeconds,
            remainingMinutes,
            remainingSecondsRemainder,
            usagePercentage,
            training.IsPublished,
            canBePublished,
            publishBlockReason,
            subTopicDtos);

        return Result<TrainingDurationInfoDto>.Success(dto);
    }
}
