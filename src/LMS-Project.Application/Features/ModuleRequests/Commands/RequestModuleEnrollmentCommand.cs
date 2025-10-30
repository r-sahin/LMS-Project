using FluentValidation;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.ModuleRequests.Commands;

public record RequestModuleEnrollmentCommand(
    Guid ModuleId,
    string RequestReason) : IRequest<Result<Guid>>;

public class RequestModuleEnrollmentCommandValidator : AbstractValidator<RequestModuleEnrollmentCommand>
{
    public RequestModuleEnrollmentCommandValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty().WithMessage("Modül ID gereklidir.");

        RuleFor(x => x.RequestReason)
            .NotEmpty().WithMessage("Talep nedeni gereklidir.")
            .MinimumLength(10).WithMessage("Talep nedeni en az 10 karakter olmalıdır.")
            .MaximumLength(500).WithMessage("Talep nedeni en fazla 500 karakter olabilir.");
    }
}

public class RequestModuleEnrollmentCommandHandler : IRequestHandler<RequestModuleEnrollmentCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public RequestModuleEnrollmentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(RequestModuleEnrollmentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // Check if module exists
        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module == null)
            return Result<Guid>.Failure("Modül bulunamadı.");

        // Check if already enrolled
        var existingEnrollment = await _unitOfWork.UserModules.GetByUserAndModuleAsync(userId, request.ModuleId, cancellationToken);
        if (existingEnrollment != null)
            return Result<Guid>.Failure("Bu modüle zaten kayıtlısınız.");

        // Check if already has pending request
        var hasPendingRequest = await _unitOfWork.ModuleRequests.HasPendingRequestAsync(userId, request.ModuleId, cancellationToken);
        if (hasPendingRequest)
            return Result<Guid>.Failure("Bu modül için zaten bekleyen bir talebiniz var.");

        // Create request
        var moduleRequest = new ModuleRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ModuleId = request.ModuleId,
            RequestReason = request.RequestReason,
            Status = "Pending",
            CreatedBy = userId.ToString(),
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.ModuleRequests.AddAsync(moduleRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Send notification to moderators
        // Bu kısmı sonra ekleyeceğiz

        return Result<Guid>.Success(moduleRequest.Id, "Kayıt talebiniz başarıyla oluşturuldu. Moderatör onayı bekleniyor.");
    }
}
