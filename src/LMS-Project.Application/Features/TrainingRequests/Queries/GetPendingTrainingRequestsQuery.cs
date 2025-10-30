using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.TrainingRequests.Queries;

public record GetPendingTrainingRequestsQuery : IRequest<Result<List<PendingTrainingRequestDto>>>;

public class PendingTrainingRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public Guid TrainingId { get; set; }
    public string TrainingName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string RequestReason { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class GetPendingTrainingRequestsQueryHandler : IRequestHandler<GetPendingTrainingRequestsQuery, Result<List<PendingTrainingRequestDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingTrainingRequestsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<PendingTrainingRequestDto>>> Handle(GetPendingTrainingRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _unitOfWork.TrainingRequests.GetPendingRequestsAsync(cancellationToken);

        if (!requests.Any())
        {
            return Result<List<PendingTrainingRequestDto>>.Failure("Bekleyen eğitim talebi bulunamadı.");
        }

        var requestDtos = requests.Select(tr => new PendingTrainingRequestDto
        {
            Id = tr.Id,
            UserId = tr.UserId,
            UserName = tr.User != null ? $"{tr.User.FirstName} {tr.User.LastName}" : "Bilinmeyen Kullanıcı",
            UserEmail = tr.User?.Email ?? "Bilinmeyen",
            TrainingId = tr.TrainingId,
            TrainingName = tr.Training?.Name ?? "Bilinmeyen Eğitim",
            ModuleName = tr.Training?.Module?.Name ?? "Bilinmeyen Modül",
            RequestReason = tr.RequestReason,
            CreatedDate = tr.CreatedDate
        }).ToList();

        return Result<List<PendingTrainingRequestDto>>.Success(requestDtos);
    }
}
