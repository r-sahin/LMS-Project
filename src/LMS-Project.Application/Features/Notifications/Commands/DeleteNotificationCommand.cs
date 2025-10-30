using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Notifications.Commands;

public record DeleteNotificationCommand(Guid NotificationId) : IRequest<Result>;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteNotificationCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification == null)
            return Result.Failure("Bildirim bulunamadı.");

        // Verify ownership
        if (notification.UserId != _currentUserService.UserId)
            return Result.Failure("Bu bildirimi silme yetkiniz yok.");

        _unitOfWork.Notifications.SoftDelete(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Bildirim başarıyla silindi.");
    }
}
