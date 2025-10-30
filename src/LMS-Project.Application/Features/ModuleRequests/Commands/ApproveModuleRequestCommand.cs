using FluentValidation;
using LMS_Project.Application.Features.Notifications.Commands;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.ModuleRequests.Commands;

public record ApproveModuleRequestCommand(
    Guid RequestId,
    string? ReviewNote = null) : IRequest<Result>;

public class ApproveModuleRequestCommandValidator : AbstractValidator<ApproveModuleRequestCommand>
{
    public ApproveModuleRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Talep ID gereklidir.");
    }
}

public class ApproveModuleRequestCommandHandler : IRequestHandler<ApproveModuleRequestCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public ApproveModuleRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Result> Handle(ApproveModuleRequestCommand request, CancellationToken cancellationToken)
    {
        var moduleRequest = await _unitOfWork.ModuleRequests.GetByIdAsync(request.RequestId, cancellationToken);
        if (moduleRequest == null)
            return Result.Failure("Talep bulunamadı.");

        if (moduleRequest.Status != "Pending")
            return Result.Failure($"Bu talep zaten {moduleRequest.Status} durumunda.");

        // Check if already enrolled (double check)
        var existingEnrollment = await _unitOfWork.UserModules.GetByUserAndModuleAsync(
            moduleRequest.UserId,
            moduleRequest.ModuleId,
            cancellationToken);

        if (existingEnrollment != null)
            return Result.Failure("Kullanıcı zaten bu modüle kayıtlı.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Update request status
            moduleRequest.Status = "Approved";
            moduleRequest.ReviewedBy = _currentUserService.UserId;
            moduleRequest.ReviewedDate = DateTime.UtcNow;
            moduleRequest.ReviewNote = request.ReviewNote;
            moduleRequest.UpdatedBy = _currentUserService.UserId.ToString();
            moduleRequest.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.ModuleRequests.Update(moduleRequest);

            // Enroll user to module
            var userModule = new UserModule
            {
                Id = Guid.NewGuid(),
                UserId = moduleRequest.UserId,
                ModuleId = moduleRequest.ModuleId,
                AssignedDate = DateTime.UtcNow,
                IsCompleted = false,
                CompletionPercentage = 0,
                CreatedBy = _currentUserService.UserId.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.UserModules.AddAsync(userModule, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification to user
            var module = await _unitOfWork.Modules.GetByIdAsync(moduleRequest.ModuleId, cancellationToken);
            await _mediator.Send(new CreateNotificationCommand(
                UserId: moduleRequest.UserId,
                Title: "Modül Kaydı Onaylandı!",
                Message: $"{module?.Name} modülüne kayıt talebiniz onaylandı. Modülü görüntüleyebilir ve eğitimler için talepte bulunabilirsiniz.",
                Type: "Success",
                RelatedEntityType: "Module",
                RelatedEntityId: moduleRequest.ModuleId,
                ActionUrl: $"/modules/{moduleRequest.ModuleId}"
            ), cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success("Talep onaylandı ve kullanıcı modüle kaydedildi.");
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
