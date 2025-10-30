using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Modules.Queries;

public record GetModuleDurationInfoQuery(Guid ModuleId) : IRequest<Result<ModuleDurationInfoDto>>;

public record ModuleDurationInfoDto(
    Guid ModuleId,
    string ModuleName,
    int ModuleTotalMinutes,
    int ModuleTotalSeconds,
    int UsedSeconds,
    int RemainingSeconds,
    int RemainingMinutes,
    int RemainingSecondsRemainder,
    decimal UsagePercentage,
    List<TrainingDurationDto> Trainings);

public record TrainingDurationDto(
    Guid TrainingId,
    string TrainingName,
    int DurationSeconds,
    int DurationMinutes);

public class GetModuleDurationInfoQueryHandler : IRequestHandler<GetModuleDurationInfoQuery, Result<ModuleDurationInfoDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetModuleDurationInfoQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ModuleDurationInfoDto>> Handle(GetModuleDurationInfoQuery request, CancellationToken cancellationToken)
    {
        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module == null)
        {
            return Result<ModuleDurationInfoDto>.Failure("Modül bulunamadı.");
        }

        var trainings = await _unitOfWork.Trainings.GetTrainingsByModuleIdAsync(request.ModuleId, cancellationToken);

        var moduleTotalSeconds = module.EstimatedDurationMinutes * 60;
        var usedSeconds = trainings.Sum(t => t.TotalDurationSeconds);
        var remainingSeconds = moduleTotalSeconds - usedSeconds;
        var remainingMinutes = remainingSeconds / 60;
        var remainingSecondsRemainder = remainingSeconds % 60;
        var usagePercentage = moduleTotalSeconds > 0
            ? Math.Round((decimal)usedSeconds / moduleTotalSeconds * 100, 2)
            : 0;

        var trainingDtos = trainings.Select(t => new TrainingDurationDto(
            t.Id,
            t.Name,
            t.TotalDurationSeconds,
            t.TotalDurationSeconds / 60)).ToList();

        var dto = new ModuleDurationInfoDto(
            module.Id,
            module.Name,
            module.EstimatedDurationMinutes,
            moduleTotalSeconds,
            usedSeconds,
            remainingSeconds,
            remainingMinutes,
            remainingSecondsRemainder,
            usagePercentage,
            trainingDtos);

        return Result<ModuleDurationInfoDto>.Success(dto);
    }
}
