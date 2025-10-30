using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace LMS_Project.Application.Features.TrainingRequests.Commands;

public record RequestTrainingEnrollmentCommand(
    Guid TrainingId,
    string RequestReason) : IRequest<Result<Guid>>;

public class RequestTrainingEnrollmentCommandValidator : AbstractValidator<RequestTrainingEnrollmentCommand>
{
    public RequestTrainingEnrollmentCommandValidator()
    {
        RuleFor(x => x.TrainingId)
            .NotEmpty().WithMessage("Eğitim ID gereklidir.");

        RuleFor(x => x.RequestReason)
            .NotEmpty().WithMessage("Talep nedeni gereklidir.")
            .MaximumLength(500).WithMessage("Talep nedeni en fazla 500 karakter olabilir.");
    }
}

public class RequestTrainingEnrollmentCommandHandler : IRequestHandler<RequestTrainingEnrollmentCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RequestTrainingEnrollmentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(RequestTrainingEnrollmentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Eğitim var mı kontrol et
        var training = await _unitOfWork.Trainings.GetByIdAsync(request.TrainingId, cancellationToken);
        if (training == null)
        {
            return Result<Guid>.Failure("Eğitim bulunamadı.");
        }

        // 2. Eğitim yayınlanmış mı kontrol et
        if (!training.IsPublished)
        {
            return Result<Guid>.Failure("Bu eğitim henüz yayınlanmamış. Yayınlanmamış eğitimler için talep oluşturamazsınız.");
        }

        // 3. Modül yayınlanmış mı kontrol et
        var module = await _unitOfWork.Modules.GetByIdAsync(training.ModuleId, cancellationToken);
        if (module == null)
        {
            return Result<Guid>.Failure("Eğitime ait modül bulunamadı.");
        }

        if (!module.IsPublished)
        {
            return Result<Guid>.Failure("Bu eğitime ait modül henüz yayınlanmamış.");
        }

        // 4. Kullanıcının modüle erişimi var mı kontrol et (UserModule var mı?)
        var userModule = await _unitOfWork.UserModules.GetByUserAndModuleAsync(userId, training.ModuleId, cancellationToken);
        if (userModule == null)
        {
            return Result<Guid>.Failure("Bu modüle erişim yetkiniz yok. Önce modül için talepte bulunmalısınız.");
        }

        // 5. Kullanıcının zaten bu eğitime erişimi var mı kontrol et
        var existingUserTraining = await _unitOfWork.UserTrainings.GetByUserAndTrainingAsync(userId, request.TrainingId, cancellationToken);
        if (existingUserTraining != null)
        {
            return Result<Guid>.Failure("Bu eğitime zaten erişiminiz var.");
        }

        // 6. Bekleyen bir talep var mı kontrol et
        var hasPendingRequest = await _unitOfWork.TrainingRequests.HasPendingRequestAsync(userId, request.TrainingId, cancellationToken);
        if (hasPendingRequest)
        {
            return Result<Guid>.Failure("Bu eğitim için zaten bekleyen bir talebiniz var.");
        }

        // 7. Talep oluştur
        var trainingRequest = new TrainingRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TrainingId = request.TrainingId,
            RequestReason = request.RequestReason,
            Status = "Pending",
            CreatedBy = userId.ToString(),
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.TrainingRequests.AddAsync(trainingRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(trainingRequest.Id, "Eğitim erişim talebiniz başarıyla oluşturuldu. Moderator onayı bekleniyor.");
    }
}
