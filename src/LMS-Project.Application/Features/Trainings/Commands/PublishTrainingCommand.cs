using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace LMS_Project.Application.Features.Trainings.Commands;

public record PublishTrainingCommand(Guid TrainingId) : IRequest<Result>;

public class PublishTrainingCommandValidator : AbstractValidator<PublishTrainingCommand>
{
    public PublishTrainingCommandValidator()
    {
        RuleFor(x => x.TrainingId)
            .NotEmpty().WithMessage("Eğitim ID gereklidir.");
    }
}

public class PublishTrainingCommandHandler : IRequestHandler<PublishTrainingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PublishTrainingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(PublishTrainingCommand request, CancellationToken cancellationToken)
    {
        var training = await _unitOfWork.Trainings.GetByIdAsync(request.TrainingId, cancellationToken);
        if (training == null)
        {
            return Result.Failure("Eğitim bulunamadı.");
        }

        if (training.IsPublished)
        {
            return Result.Failure("Eğitim zaten yayınlanmış.");
        }

        // ⚠️ KRİTİK KONTROL: Alt başlıkların toplam süresi eğitim süresine EŞİT olmalı
        var subTopics = await _unitOfWork.SubTopics.GetSubTopicsByTrainingIdAsync(request.TrainingId, cancellationToken);

        if (!subTopics.Any())
        {
            return Result.Failure("Eğitim yayınlanamaz! En az bir alt başlık eklemelisiniz.");
        }

        var totalSubTopicSeconds = subTopics.Sum(st => st.MinimumDurationSeconds);

        if (totalSubTopicSeconds != training.TotalDurationSeconds)
        {
            var difference = training.TotalDurationSeconds - totalSubTopicSeconds;
            var diffMinutes = Math.Abs(difference) / 60;
            var diffSeconds = Math.Abs(difference) % 60;

            string message;
            if (difference > 0)
            {
                message = $"Eğitim yayınlanamaz! Alt başlıkların toplam süresi eğitim süresinden KISA. " +
                         $"Eğitim süresi: {training.TotalDurationSeconds} saniye. " +
                         $"Alt başlıklar toplamı: {totalSubTopicSeconds} saniye. " +
                         $"Eksik süre: {diffMinutes} dakika {diffSeconds} saniye.";
            }
            else
            {
                message = $"Eğitim yayınlanamaz! Alt başlıkların toplam süresi eğitim süresinden UZUN. " +
                         $"Eğitim süresi: {training.TotalDurationSeconds} saniye. " +
                         $"Alt başlıklar toplamı: {totalSubTopicSeconds} saniye. " +
                         $"Fazla süre: {diffMinutes} dakika {diffSeconds} saniye.";
            }

            return Result.Failure(message);
        }

        // ✅ Tüm kontroller geçti, eğitimi yayınla
        training.IsPublished = true;
        training.PublishedDate = DateTime.UtcNow;
        training.PublishedBy = _currentUserService.UserId.ToString();
        training.UpdatedBy = _currentUserService.UserId.ToString();
        training.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Trainings.Update(training);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var subTopicCount = subTopics.Count();
        var successMessage = $"Eğitim başarıyla yayınlandı! Toplam {subTopicCount} alt başlık ile {training.TotalDurationSeconds} saniye süre.";
        return Result.Success(successMessage);
    }
}
