using FluentValidation;
using LMS_Project.Application.Features.Notifications.Commands;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Trainings.Commands;

public record ResetUserTrainingCommand(
    Guid UserId,
    Guid TrainingId,
    string ResetReason) : IRequest<Result>;

public class ResetUserTrainingCommandValidator : AbstractValidator<ResetUserTrainingCommand>
{
    public ResetUserTrainingCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID gereklidir.");

        RuleFor(x => x.TrainingId)
            .NotEmpty().WithMessage("Eğitim ID gereklidir.");

        RuleFor(x => x.ResetReason)
            .NotEmpty().WithMessage("Sıfırlama nedeni gereklidir.")
            .MinimumLength(10).WithMessage("Sıfırlama nedeni en az 10 karakter olmalıdır.")
            .MaximumLength(500).WithMessage("Sıfırlama nedeni en fazla 500 karakter olabilir.");
    }
}

public class ResetUserTrainingCommandHandler : IRequestHandler<ResetUserTrainingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public ResetUserTrainingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Result> Handle(ResetUserTrainingCommand request, CancellationToken cancellationToken)
    {
        var moderatorId = _currentUserService.UserId;

        // 1. Eğitimi kontrol et
        var training = await _unitOfWork.Trainings.GetTrainingWithSubTopicsAsync(request.TrainingId, cancellationToken);
        if (training == null)
        {
            return Result.Failure("Eğitim bulunamadı.");
        }

        // 2. UserTraining kaydını kontrol et
        var userTraining = await _unitOfWork.UserTrainings.GetByUserAndTrainingAsync(
            request.UserId,
            request.TrainingId,
            cancellationToken);

        if (userTraining == null)
        {
            return Result.Failure("Kullanıcının bu eğitime erişimi yok.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 3. UserTraining'i sıfırla (kayıt kalır, sadece progress sıfırlanır)
            userTraining.IsCompleted = false;
            userTraining.CompletionPercentage = 0;
            userTraining.CompletedDate = null;
            userTraining.UpdatedBy = moderatorId.ToString();
            userTraining.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.UserTrainings.Update(userTraining);

            // 4. Bu eğitime ait TÜM UserProgress kayıtlarını sil
            var userProgresses = await _unitOfWork.UserProgresses
                .GetProgressesByUserAndTrainingAsync(request.UserId, request.TrainingId, cancellationToken);

            foreach (var progress in userProgresses)
            {
                _unitOfWork.UserProgresses.Remove(progress);
            }

            // 5. Bu eğitime ait sertifikayı sil (varsa)
            var trainingCertificates = await _unitOfWork.Certificates
                .GetCertificatesByUserAndTrainingAsync(request.UserId, request.TrainingId, cancellationToken);

            foreach (var cert in trainingCertificates)
            {
                _unitOfWork.Certificates.Remove(cert);
            }

            // 6. Eğer modül bu yüzünden tamamlanmışsa, modül sertifikasını da sil
            var userModule = await _unitOfWork.UserModules.GetByUserAndModuleAsync(
                request.UserId,
                training.ModuleId,
                cancellationToken);

            if (userModule != null && userModule.IsCompleted)
            {
                // Modülün diğer eğitimlerini kontrol et
                var moduleTrainings = await _unitOfWork.Trainings.GetTrainingsByModuleIdAsync(training.ModuleId, cancellationToken);
                var userTrainingsInModule = new List<bool>();

                foreach (var modTraining in moduleTrainings)
                {
                    if (modTraining.Id == request.TrainingId)
                    {
                        // Şu an sıfırlanan eğitim - tamamlanmamış sayılır
                        userTrainingsInModule.Add(false);
                    }
                    else
                    {
                        var otherUserTraining = await _unitOfWork.UserTrainings.GetByUserAndTrainingAsync(
                            request.UserId,
                            modTraining.Id,
                            cancellationToken);

                        userTrainingsInModule.Add(otherUserTraining?.IsCompleted ?? false);
                    }
                }

                // Eğer tüm eğitimler tamamlanmamışsa, modül incomplete olmalı
                if (userTrainingsInModule.Any(completed => !completed))
                {
                    userModule.IsCompleted = false;
                    userModule.CompletionPercentage = 0; // Veya yeniden hesaplanabilir
                    userModule.CompletedDate = null;
                    userModule.UpdatedBy = moderatorId.ToString();
                    userModule.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.UserModules.Update(userModule);

                    // Modül sertifikasını sil
                    var moduleCertificates = await _unitOfWork.Certificates
                        .GetCertificatesByUserAndModuleAsync(request.UserId, training.ModuleId, cancellationToken);

                    foreach (var cert in moduleCertificates)
                    {
                        _unitOfWork.Certificates.Remove(cert);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Kullanıcıya bildirim gönder
            await _mediator.Send(new CreateNotificationCommand(
                UserId: request.UserId,
                Title: "Eğitim Sıfırlandı",
                Message: $"{training.Name} eğitimi sıfırlandı. Neden: {request.ResetReason}. Eğitimi tekrar tamamlamanız gerekmektedir.",
                Type: "Warning",
                RelatedEntityType: "Training",
                RelatedEntityId: request.TrainingId,
                ActionUrl: $"/trainings/{request.TrainingId}"
            ), cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success($"{training.Name} eğitimi başarıyla sıfırlandı. Kullanıcı bilgilendirildi.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Failure($"Eğitim sıfırlama işlemi başarısız: {ex.Message}");
        }
    }
}
