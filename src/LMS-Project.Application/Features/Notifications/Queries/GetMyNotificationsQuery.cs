using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Notifications.Queries;

public record GetMyNotificationsQuery(bool OnlyUnread = false) : IRequest<Result<IEnumerable<NotificationDto>>>;

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, Result<IEnumerable<NotificationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyNotificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = request.OnlyUnread
            ? await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(_currentUserService.UserId, cancellationToken)
            : await _unitOfWork.Notifications.GetNotificationsByUserIdAsync(_currentUserService.UserId, cancellationToken);

        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            ReadDate = n.ReadDate,
            RelatedEntityType = n.RelatedEntityType,
            RelatedEntityId = n.RelatedEntityId,
            ActionUrl = n.ActionUrl,
            CreatedDate = n.CreatedDate
        }).OrderByDescending(n => n.CreatedDate).ToList();

        return Result<IEnumerable<NotificationDto>>.Success(dtos);
    }
}
