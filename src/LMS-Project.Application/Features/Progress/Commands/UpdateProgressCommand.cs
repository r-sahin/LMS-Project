using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using MediatR;

namespace LMS_Project.Application.Features.Progress.Commands;

public record UpdateProgressCommand(
    Guid SubTopicId,
    int DurationSeconds,
    string SessionId,
    string? IpAddress = null,
    string? DeviceInfo = null) : IRequest<Result<UserProgressDto>>;

public class UpdateProgressCommandHandler : IRequestHandler<UpdateProgressCommand, Result<UserProgressDto>>
{
    private readonly IProgressService _progressService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProgressCommandHandler(
        IProgressService progressService,
        ICurrentUserService currentUserService)
    {
        _progressService = progressService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserProgressDto>> Handle(
        UpdateProgressCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId == Guid.Empty)
        {
            return Result<UserProgressDto>.Failure("Kullanıcı kimliği bulunamadı.");
        }

        return await _progressService.UpdateProgressAsync(
            userId,
            request.SubTopicId,
            request.DurationSeconds,
            request.SessionId,
            request.IpAddress,
            request.DeviceInfo,
            cancellationToken);
    }
}
