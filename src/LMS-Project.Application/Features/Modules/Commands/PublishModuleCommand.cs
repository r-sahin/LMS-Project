using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace LMS_Project.Application.Features.Modules.Commands;

public record PublishModuleCommand(Guid ModuleId) : IRequest<Result>;

public class PublishModuleCommandValidator : AbstractValidator<PublishModuleCommand>
{
    public PublishModuleCommandValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty().WithMessage("Modül ID gereklidir.");
    }
}

public class PublishModuleCommandHandler : IRequestHandler<PublishModuleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PublishModuleCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(PublishModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module == null)
        {
            return Result.Failure("Modül bulunamadı.");
        }

        if (module.IsPublished)
        {
            return Result.Failure("Modül zaten yayınlanmış.");
        }

        // ⚠️ KRİTİK KONTROL: En az bir yayınlanmış eğitim olmalı
        var trainings = await _unitOfWork.Trainings.GetTrainingsByModuleIdAsync(request.ModuleId, cancellationToken);
        var publishedTrainings = trainings.Where(t => t.IsPublished).ToList();

        if (!publishedTrainings.Any())
        {
            return Result.Failure("Modül yayınlanamaz! En az bir yayınlanmış eğitim olmalıdır.");
        }

        // ⚠️ Yayınlanmış eğitimlerin toplam süresi modül süresine eşit olmalı
        var totalPublishedSeconds = publishedTrainings.Sum(t => t.TotalDurationSeconds);
        var moduleTotalSeconds = module.EstimatedDurationMinutes * 60;

        if (totalPublishedSeconds != moduleTotalSeconds)
        {
            var difference = moduleTotalSeconds - totalPublishedSeconds;
            var diffMinutes = Math.Abs(difference) / 60;
            var diffSeconds = Math.Abs(difference) % 60;

            string errorMessage;
            if (difference > 0)
            {
                errorMessage = $"Modül yayınlanamaz! Yayınlanmış eğitimlerin toplam süresi modül süresinden KISA. " +
                             $"Modül süresi: {module.EstimatedDurationMinutes} dakika. " +
                             $"Yayınlanmış eğitimler toplamı: {totalPublishedSeconds / 60} dakika. " +
                             $"Eksik: {diffMinutes} dakika {diffSeconds} saniye.";
            }
            else
            {
                errorMessage = $"Modül yayınlanamaz! Yayınlanmış eğitimlerin toplam süresi modül süresinden UZUN. " +
                             $"Modül süresi: {module.EstimatedDurationMinutes} dakika. " +
                             $"Yayınlanmış eğitimler toplamı: {totalPublishedSeconds / 60} dakika. " +
                             $"Fazla: {diffMinutes} dakika {diffSeconds} saniye.";
            }

            return Result.Failure(errorMessage);
        }

        // ✅ Tüm kontroller geçti, modülü yayınla
        module.IsPublished = true;
        module.PublishedDate = DateTime.UtcNow;
        module.PublishedBy = _currentUserService.UserId.ToString();
        module.UpdatedBy = _currentUserService.UserId.ToString();
        module.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Modules.Update(module);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var successMessage = $"Modül başarıyla yayınlandı! {publishedTrainings.Count} yayınlanmış eğitim ile toplamda {module.EstimatedDurationMinutes} dakika.";
        return Result.Success(successMessage);
    }
}
