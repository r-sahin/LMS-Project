using FluentValidation;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Notifications.Commands;

public record CreateNotificationCommand(
    Guid UserId,
    string Title,
    string Message,
    string Type,
    string? RelatedEntityType = null,
    Guid? RelatedEntityId = null,
    string? ActionUrl = null) : IRequest<Result<Guid>>;

public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => t == "Info" || t == "Success" || t == "Warning" || t == "Error" || t == "Achievement")
            .WithMessage("Type must be Info, Success, Warning, Error, or Achievement.");
    }
}

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateNotificationCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            IsRead = false,
            RelatedEntityType = request.RelatedEntityType,
            RelatedEntityId = request.RelatedEntityId,
            ActionUrl = request.ActionUrl,
            CreatedBy = _currentUserService.UserId.ToString(),
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(notification.Id, "Bildirim başarıyla oluşturuldu.");
    }
}
