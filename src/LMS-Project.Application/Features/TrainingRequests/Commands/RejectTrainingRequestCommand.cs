using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace LMS_Project.Application.Features.TrainingRequests.Commands;

public record RejectTrainingRequestCommand(
    Guid RequestId,
    string ReviewNote) : IRequest<Result>;

public class RejectTrainingRequestCommandValidator : AbstractValidator<RejectTrainingRequestCommand>
{
    public RejectTrainingRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Talep ID gereklidir.");

        RuleFor(x => x.ReviewNote)
            .NotEmpty().WithMessage("Ret nedeni gereklidir.")
            .MaximumLength(500).WithMessage("Ret nedeni en fazla 500 karakter olabilir.");
    }
}

public class RejectTrainingRequestCommandHandler : IRequestHandler<RejectTrainingRequestCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RejectTrainingRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RejectTrainingRequestCommand request, CancellationToken cancellationToken)
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

        // 3. Eğitim bilgisini al (bildirim için)
        var training = await _unitOfWork.Trainings.GetByIdAsync(trainingRequest.TrainingId, cancellationToken);
        var trainingName = training?.Name ?? "Bilinmeyen Eğitim";

        // 4. Transaction başlat
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 5. Talep durumunu güncelle
            trainingRequest.Status = "Rejected";
            trainingRequest.ReviewedBy = moderatorId;
            trainingRequest.ReviewedDate = DateTime.UtcNow;
            trainingRequest.ReviewNote = request.ReviewNote;
            trainingRequest.UpdatedBy = moderatorId.ToString();
            trainingRequest.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.TrainingRequests.Update(trainingRequest);

            // 6. Bildirim oluştur
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = trainingRequest.UserId,
                Title = "Eğitim Talebiniz Reddedildi",
                Message = $"{trainingName} eğitimine erişim talebiniz reddedildi. Ret nedeni: {request.ReviewNote}",
                Type = "TrainingRequestRejected",
                IsRead = false,
                CreatedBy = moderatorId.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);

            // 7. Transaction commit
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success($"Eğitim talebi başarıyla reddedildi.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
