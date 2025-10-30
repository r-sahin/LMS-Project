using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace LMS_Project.Application.Features.TrainingRequests.Commands;

public record ApproveTrainingRequestCommand(
    Guid RequestId,
    string? ReviewNote = null) : IRequest<Result>;

public class ApproveTrainingRequestCommandValidator : AbstractValidator<ApproveTrainingRequestCommand>
{
    public ApproveTrainingRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Talep ID gereklidir.");

        RuleFor(x => x.ReviewNote)
            .MaximumLength(500).WithMessage("İnceleme notu en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.ReviewNote));
    }
}

public class ApproveTrainingRequestCommandHandler : IRequestHandler<ApproveTrainingRequestCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ApproveTrainingRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ApproveTrainingRequestCommand request, CancellationToken cancellationToken)
    {
        var moderatorId = _currentUserService.UserId;

        // 1. Talep var mı kontrol et
        var trainingRequest = await _unitOfWork.TrainingRequests.GetByIdAsync(request.RequestId, cancellationToken);
        if (trainingRequest == null)
        {
            return Result.Failure("Eğitim talebi bulunamadı.");
        }

        // 2. Talep durumu kontrol et
        if (trainingRequest.Status != "Pending")
        {
            return Result.Failure($"Bu talep zaten işleme alınmış. Mevcut durum: {trainingRequest.Status}");
        }

        // 3. Eğitim var mı kontrol et
        var training = await _unitOfWork.Trainings.GetByIdAsync(trainingRequest.TrainingId, cancellationToken);
        if (training == null)
        {
            return Result.Failure("Eğitim bulunamadı.");
        }

        // 4. Eğitim hala yayınlanmış mı kontrol et
        if (!training.IsPublished)
        {
            return Result.Failure("Bu eğitim artık yayınlanmamış durumda. Talep onaylanamaz.");
        }

        // 5. Kullanıcının modüle hala erişimi var mı kontrol et
        var userModule = await _unitOfWork.UserModules.GetByUserAndModuleAsync(
            trainingRequest.UserId,
            training.ModuleId,
            cancellationToken);

        if (userModule == null)
        {
            return Result.Failure("Kullanıcının modül erişimi kaldırılmış. Talep onaylanamaz.");
        }

        // 6. Kullanıcının zaten bu eğitime erişimi var mı kontrol et
        var existingUserTraining = await _unitOfWork.UserTrainings.GetByUserAndTrainingAsync(
            trainingRequest.UserId,
            trainingRequest.TrainingId,
            cancellationToken);

        if (existingUserTraining != null)
        {
            return Result.Failure("Kullanıcı bu eğitime zaten erişime sahip.");
        }

        // 7. Transaction başlat
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 8. Talep durumunu güncelle
            trainingRequest.Status = "Approved";
            trainingRequest.ReviewedBy = moderatorId;
            trainingRequest.ReviewedDate = DateTime.UtcNow;
            trainingRequest.ReviewNote = request.ReviewNote;
            trainingRequest.UpdatedBy = moderatorId.ToString();
            trainingRequest.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.TrainingRequests.Update(trainingRequest);

            // 9. UserTraining oluştur
            var userTraining = new UserTraining
            {
                Id = Guid.NewGuid(),
                UserId = trainingRequest.UserId,
                TrainingId = trainingRequest.TrainingId,
                IsCompleted = false,
                CompletionPercentage = 0,
                CreatedBy = moderatorId.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.UserTrainings.AddAsync(userTraining, cancellationToken);

            // 10. Bildirim oluştur
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = trainingRequest.UserId,
                Title = "Eğitim Talebiniz Onaylandı",
                Message = $"{training.Name} eğitimine erişim talebiniz onaylandı. Artık eğitime erişebilirsiniz.",
                Type = "TrainingRequestApproved",
                IsRead = false,
                CreatedBy = moderatorId.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);

            // 11. Transaction commit
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success($"Eğitim talebi başarıyla onaylandı. Kullanıcı artık '{training.Name}' eğitimine erişebilir.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
