using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS_Project.Application.Features.Trainings.Commands;

public record UpdateTrainingCommand(
    Guid Id,
    string Name,
    string? Description,
    int TotalDurationSeconds,
    bool IsActive,
    IFormFile? ThumbnailFile,
    IFormFile? VideoIntroFile) : IRequest<Result>;

public class UpdateTrainingCommandHandler : IRequestHandler<UpdateTrainingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public UpdateTrainingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result> Handle(UpdateTrainingCommand request, CancellationToken cancellationToken)
    {
        var training = await _unitOfWork.Trainings.GetByIdAsync(request.Id, cancellationToken);
        if (training == null) return Result.Failure("Eğitim bulunamadı.");

        // Modülü kontrol et
        var module = await _unitOfWork.Modules.GetByIdAsync(training.ModuleId, cancellationToken);
        if (module == null)
        {
            return Result.Failure("Modül bulunamadı.");
        }

        // Eğer süre değişiyorsa, toplam süre kontrolü yap
        if (training.TotalDurationSeconds != request.TotalDurationSeconds)
        {
            // Mevcut eğitimlerin toplam süresini hesapla (güncellenecek eğitim HARİÇ)
            var trainings = await _unitOfWork.Trainings.GetTrainingsByModuleIdAsync(training.ModuleId, cancellationToken);
            var currentTotalSeconds = trainings
                .Where(t => t.Id != request.Id) // Güncellenecek eğitimi çıkar
                .Sum(t => t.TotalDurationSeconds);

            // Modül süresi (dakika -> saniye)
            var moduleTotalSeconds = module.EstimatedDurationMinutes * 60;

            // Yeni eğitim süresi eklenince toplam süre kontrol
            var newTotalSeconds = currentTotalSeconds + request.TotalDurationSeconds;

            if (newTotalSeconds > moduleTotalSeconds)
            {
                var remainingSeconds = moduleTotalSeconds - currentTotalSeconds;
                var remainingMinutes = remainingSeconds / 60;
                var remainingSecondsRemainder = remainingSeconds % 60;

                return Result.Failure(
                    $"Eğitim süresi modül süresini aşıyor! " +
                    $"Modül toplam süresi: {module.EstimatedDurationMinutes} dakika ({moduleTotalSeconds} saniye). " +
                    $"Diğer eğitimler toplamı: {currentTotalSeconds} saniye. " +
                    $"Kalan süre: {remainingMinutes} dakika {remainingSecondsRemainder} saniye. " +
                    $"Girdiğiniz süre: {request.TotalDurationSeconds} saniye.");
            }
        }

        training.Name = request.Name;
        training.Description = request.Description;
        training.TotalDurationSeconds = request.TotalDurationSeconds;
        training.IsActive = request.IsActive;
        training.UpdatedBy = _currentUserService.UserId.ToString();
        training.UpdatedDate = DateTime.UtcNow;

        if (request.ThumbnailFile != null)
        {
            if (!string.IsNullOrEmpty(training.ThumbnailPath))
                await _fileService.DeleteFileAsync(training.ThumbnailPath, cancellationToken);

            var result = await _fileService.UploadFileAsync(request.ThumbnailFile, "images/trainings", cancellationToken);
            if (result.IsSuccess) training.ThumbnailPath = result.Data;
        }

        if (request.VideoIntroFile != null)
        {
            if (!string.IsNullOrEmpty(training.VideoIntroPath))
                await _fileService.DeleteFileAsync(training.VideoIntroPath, cancellationToken);

            var result = await _fileService.UploadFileAsync(request.VideoIntroFile, "videos/trainings", cancellationToken);
            if (result.IsSuccess) training.VideoIntroPath = result.Data;
        }

        _unitOfWork.Trainings.Update(training);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Eğitim başarıyla güncellendi.");
    }
}
