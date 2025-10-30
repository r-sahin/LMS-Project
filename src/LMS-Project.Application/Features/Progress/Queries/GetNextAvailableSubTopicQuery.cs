using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using MediatR;

namespace LMS_Project.Application.Features.Progress.Queries;

public record GetNextAvailableSubTopicQuery(Guid TrainingId) : IRequest<Result<SubTopicDto>>;

public class GetNextAvailableSubTopicQueryHandler : IRequestHandler<GetNextAvailableSubTopicQuery, Result<SubTopicDto>>
{
    private readonly IProgressService _progressService;
    private readonly ICurrentUserService _currentUserService;

    public GetNextAvailableSubTopicQueryHandler(
        IProgressService progressService,
        ICurrentUserService currentUserService)
    {
        _progressService = progressService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SubTopicDto>> Handle(
        GetNextAvailableSubTopicQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId == Guid.Empty)
        {
            return Result<SubTopicDto>.Failure("Kullanıcı kimliği bulunamadı.");
        }

        return await _progressService.GetNextAvailableSubTopicAsync(
            userId,
            request.TrainingId,
            cancellationToken);
    }
}
