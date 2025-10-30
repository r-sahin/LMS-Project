using FluentValidation;
using LMS_Project.Application.Features.Notifications.Commands;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.ModuleRequests.Commands;

public record RejectModuleRequestCommand(
    Guid RequestId,
    string RejectReason) : IRequest<Result>;

public class RejectModuleRequestCommandValidator : AbstractValidator<RejectModuleRequestCommand>
{
    public RejectModuleRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Talep ID gereklidir.");

        RuleFor(x => x.RejectReason)
            .NotEmpty().WithMessage("Ret nedeni gereklidir.")
            .MinimumLength(10).WithMessage("Ret nedeni en az 10 karakter olmalıdır.")
            .MaximumLength(500).WithMessage("Ret nedeni en fazla 500 karakter olabilir.");
    }
}

public class RejectModuleRequestCommandHandler : IRequestHandler<RejectModuleRequestCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public RejectModuleRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Result> Handle(RejectModuleRequestCommand request, CancellationToken cancellationToken)
    {
        var moduleRequest = await _unitOfWork.ModuleRequests.GetByIdAsync(request.RequestId, cancellationToken);
        if (moduleRequest == null)
            return Result.Failure("Talep bulunamadı.");

        if (moduleRequest.Status != "Pending")
            return Result.Failure($"Bu talep zaten {moduleRequest.Status} durumunda.");

        // Update request status
        moduleRequest.Status = "Rejected";
        moduleRequest.ReviewedBy = _currentUserService.UserId;
        moduleRequest.ReviewedDate = DateTime.UtcNow;
        moduleRequest.ReviewNote = request.RejectReason;
        moduleRequest.UpdatedBy = _currentUserService.UserId.ToString();
        moduleRequest.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.ModuleRequests.Update(moduleRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification to user
        var module = await _unitOfWork.Modules.GetByIdAsync(moduleRequest.ModuleId, cancellationToken);
        await _mediator.Send(new CreateNotificationCommand(
            UserId: moduleRequest.UserId,
            Title: "Modül Kaydı Reddedildi",
            Message: $"{module?.Name} modülüne kayıt talebiniz reddedildi. Neden: {request.RejectReason}",
            Type: "Warning",
            RelatedEntityType: "Module",
            RelatedEntityId: moduleRequest.ModuleId,
            ActionUrl: null
        ), cancellationToken);

        return Result.Success("Talep reddedildi ve kullanıcıya bildirim gönderildi.");
    }
}
