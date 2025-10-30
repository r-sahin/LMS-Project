using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.TrainingRequests.Queries;

public record GetMyTrainingRequestsQuery : IRequest<Result<List<TrainingRequestDto>>>;

public class TrainingRequestDto
{
    public Guid Id { get; set; }
    public Guid TrainingId { get; set; }
    public string TrainingName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string RequestReason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewNote { get; set; }
    public string? ReviewerName { get; set; }
}

public class GetMyTrainingRequestsQueryHandler : IRequestHandler<GetMyTrainingRequestsQuery, Result<List<TrainingRequestDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyTrainingRequestsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<TrainingRequestDto>>> Handle(GetMyTrainingRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var requests = await _unitOfWork.TrainingRequests.GetRequestsByUserIdAsync(userId, cancellationToken);

        if (!requests.Any())
        {
            return Result<List<TrainingRequestDto>>.Failure("Hiç eğitim talebiniz bulunamadı.");
        }

        var requestDtos = requests.Select(tr => new TrainingRequestDto
        {
            Id = tr.Id,
            TrainingId = tr.TrainingId,
            TrainingName = tr.Training?.Name ?? "Bilinmeyen Eğitim",
            ModuleName = tr.Training?.Module?.Name ?? "Bilinmeyen Modül",
            RequestReason = tr.RequestReason,
            Status = tr.Status,
            CreatedDate = tr.CreatedDate,
            ReviewedDate = tr.ReviewedDate,
            ReviewNote = tr.ReviewNote,
            ReviewerName = tr.Reviewer != null ? $"{tr.Reviewer.FirstName} {tr.Reviewer.LastName}" : null
        }).ToList();

        return Result<List<TrainingRequestDto>>.Success(requestDtos);
    }
}
