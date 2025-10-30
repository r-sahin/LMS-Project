using FluentValidation;
using LMS_Project.Application.Features.Notifications.Commands;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Modules.Commands;

public record ResetUserModuleCommand(
    Guid UserId,
    Guid ModuleId,
    string ResetReason) : IRequest<Result>;

public class ResetUserModuleCommandValidator : AbstractValidator<ResetUserModuleCommand>
{
    public ResetUserModuleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID gereklidir.");

        RuleFor(x => x.ModuleId)
            .NotEmpty().WithMessage("Modül ID gereklidir.");

        RuleFor(x => x.ResetReason)
            .NotEmpty().WithMessage("Sıfırlama nedeni gereklidir.")
            .MinimumLength(10).WithMessage("Sıfırlama nedeni en az 10 karakter olmalıdır.")
            .MaximumLength(500).WithMessage("Sıfırlama nedeni en fazla 500 karakter olabilir.");
    }
}

public class ResetUserModuleCommandHandler : IRequestHandler<ResetUserModuleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public ResetUserModuleCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Result> Handle(ResetUserModuleCommand request, CancellationToken cancellationToken)
    {
        var moderatorId = _currentUserService.UserId;

        // 1. Modülü kontrol et
        var module = await _unitOfWork.Modules.GetModuleWithTrainingsAsync(request.ModuleId, cancellationToken);
        if (module == null)
        {
            return Result.Failure("Modül bulunamadı.");
        }

        // 2. UserModule kaydını kontrol et
        var userModule = await _unitOfWork.UserModules.GetByUserAndModuleAsync(
            request.UserId,
            request.ModuleId,
            cancellationToken);

        if (userModule == null)
        {
            return Result.Failure("Kullanıcının bu modüle erişimi yok.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 3. UserModule'i sıfırla (kayıt kalır, sadece progress sıfırlanır)
            userModule.IsCompleted = false;
            userModule.CompletionPercentage = 0;
            userModule.CompletedDate = null;
            userModule.UpdatedBy = moderatorId.ToString();
            userModule.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.UserModules.Update(userModule);

            // 4. Modüldeki TÜM eğitimleri getir ve sıfırla
            var trainingsInModule = await _unitOfWork.Trainings.GetTrainingsByModuleIdAsync(request.ModuleId, cancellationToken);

            foreach (var training in trainingsInModule)
            {
                // UserTraining kaydını sıfırla
                var userTraining = await _unitOfWork.UserTrainings.GetByUserAndTrainingAsync(
                    request.UserId,
                    training.Id,
                    cancellationToken);

                if (userTraining != null)
                {
                    userTraining.IsCompleted = false;
                    userTraining.CompletionPercentage = 0;
                    userTraining.CompletedDate = null;
                    userTraining.UpdatedBy = moderatorId.ToString();
                    userTraining.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.UserTrainings.Update(userTraining);
                }

                // Bu eğitime ait TÜM UserProgress kayıtlarını sil
                var userProgresses = await _unitOfWork.UserProgresses
                    .GetProgressesByUserAndTrainingAsync(request.UserId, training.Id, cancellationToken);

                foreach (var progress in userProgresses)
                {
                    _unitOfWork.UserProgresses.Remove(progress);
                }

                // Bu eğitime ait sertifikaları sil
                var trainingCertificates = await _unitOfWork.Certificates
                    .GetCertificatesByUserAndTrainingAsync(request.UserId, training.Id, cancellationToken);

                foreach (var cert in trainingCertificates)
                {
                    _unitOfWork.Certificates.Remove(cert);
                }
            }

            // 5. Modül sertifikalarını sil
            var moduleCertificates = await _unitOfWork.Certificates
                .GetCertificatesByUserAndModuleAsync(request.UserId, request.ModuleId, cancellationToken);

            foreach (var cert in moduleCertificates)
            {
                _unitOfWork.Certificates.Remove(cert);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Kullanıcıya bildirim gönder
            await _mediator.Send(new CreateNotificationCommand(
                UserId: request.UserId,
                Title: "Modül Sıfırlandı",
                Message: $"{module.Name} modülü ve içindeki tüm eğitimler sıfırlandı. Neden: {request.ResetReason}. Tüm eğitimleri tekrar tamamlamanız gerekmektedir.",
                Type: "Warning",
                RelatedEntityType: "Module",
                RelatedEntityId: request.ModuleId,
                ActionUrl: $"/modules/{request.ModuleId}"
            ), cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success($"{module.Name} modülü ve içindeki {trainingsInModule.Count()} eğitim başarıyla sıfırlandı. Kullanıcı bilgilendirildi.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Failure($"Modül sıfırlama işlemi başarısız: {ex.Message}");
        }
    }
}
