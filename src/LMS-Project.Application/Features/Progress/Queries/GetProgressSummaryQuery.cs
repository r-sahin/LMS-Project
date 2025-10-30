using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using MediatR;

namespace LMS_Project.Application.Features.Progress.Queries;

public record GetProgressSummaryQuery(Guid ModuleId) : IRequest<Result<ProgressSummaryDto>>;

public class GetProgressSummaryQueryHandler : IRequestHandler<GetProgressSummaryQuery, Result<ProgressSummaryDto>>
{
    private readonly IProgressService _progressService;
    private readonly ICurrentUserService _currentUserService;

    public GetProgressSummaryQueryHandler(
        IProgressService progressService,
        ICurrentUserService currentUserService)
    {
        _progressService = progressService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProgressSummaryDto>> Handle(
        GetProgressSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId == Guid.Empty)
        {
            return Result<ProgressSummaryDto>.Failure("Kullanıcı kimliği bulunamadı.");
        }

        return await _progressService.GetModuleProgressSummaryAsync(
            userId,
            request.ModuleId,
            cancellationToken);
    }
}
