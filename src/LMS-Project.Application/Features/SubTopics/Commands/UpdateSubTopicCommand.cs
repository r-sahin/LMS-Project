using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS_Project.Application.Features.SubTopics.Commands;

public record UpdateSubTopicCommand(
    Guid Id,
    string Name,
    string? Description,
    int MinimumDurationSeconds,
    bool IsMandatory,
    bool IsActive,
    IFormFile? ZipFile,
    IFormFile? ThumbnailFile) : IRequest<Result>;

public class UpdateSubTopicCommandHandler : IRequestHandler<UpdateSubTopicCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public UpdateSubTopicCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result> Handle(UpdateSubTopicCommand request, CancellationToken cancellationToken)
    {
        var subTopic = await _unitOfWork.SubTopics.GetByIdWithIncludesAsync(request.Id, cancellationToken, st => st.Training);
        if (subTopic == null) return Result.Failure("Alt başlık bulunamadı.");

        // ⚠️ KRİTİK KONTROL: Süre değişiyorsa eğitim süresini kontrol et
        if (subTopic.MinimumDurationSeconds != request.MinimumDurationSeconds)
        {
            var training = subTopic.Training;
            var subTopics = await _unitOfWork.SubTopics.GetSubTopicsByTrainingIdAsync(subTopic.TrainingId, cancellationToken);
            var currentTotalSeconds = subTopics
                .Where(st => st.Id != request.Id) // Güncellenecek alt başlığı çıkar
                .Sum(st => st.MinimumDurationSeconds);

            var newTotalSeconds = currentTotalSeconds + request.MinimumDurationSeconds;

            if (newTotalSeconds > training.TotalDurationSeconds)
            {
                var remainingSeconds = training.TotalDurationSeconds - currentTotalSeconds;
                var remainingMinutes = remainingSeconds / 60;
                var remainingSecondsRemainder = remainingSeconds % 60;

                return Result.Failure(
                    $"Alt başlık süresi eğitim süresini aşıyor! " +
                    $"Eğitim toplam süresi: {training.TotalDurationSeconds} saniye. " +
                    $"Diğer alt başlıklar toplamı: {currentTotalSeconds} saniye. " +
                    $"Kalan süre: {remainingMinutes} dakika {remainingSecondsRemainder} saniye. " +
                    $"Girdiğiniz süre: {request.MinimumDurationSeconds} saniye.");
            }
        }

        subTopic.Name = request.Name;
        subTopic.Description = request.Description;
        subTopic.MinimumDurationSeconds = request.MinimumDurationSeconds;
        subTopic.IsMandatory = request.IsMandatory;
        subTopic.IsActive = request.IsActive;
        subTopic.UpdatedBy = _currentUserService.UserId.ToString();
        subTopic.UpdatedDate = DateTime.UtcNow;

        // Yeni ZIP yüklenmişse
        if (request.ZipFile != null)
        {
            // Eski dosyaları sil
            await _fileService.DeleteSubTopicFilesAsync(
                subTopic.Training.ModuleId,
                subTopic.TrainingId,
                subTopic.Id,
                cancellationToken);

            // Yeni ZIP'i yükle ve ayıkla
            var zipResult = await _fileService.UploadAndExtractZipAsync(
                request.ZipFile,
                subTopic.Training.ModuleId,
                subTopic.TrainingId,
                subTopic.Id,
                cancellationToken);

            if (!zipResult.IsSuccess) return Result.Failure(zipResult.Message);

            subTopic.HtmlFilePath = zipResult.Data!;
            subTopic.ZipFilePath = $"/content/modules/{subTopic.Training.ModuleId}/trainings/{subTopic.TrainingId}/subtopics/{subTopic.Id}/content.zip";
        }

        if (request.ThumbnailFile != null)
        {
            if (!string.IsNullOrEmpty(subTopic.ThumbnailPath))
                await _fileService.DeleteFileAsync(subTopic.ThumbnailPath, cancellationToken);

            var thumbResult = await _fileService.UploadFileAsync(request.ThumbnailFile, "images/subtopics", cancellationToken);
            if (thumbResult.IsSuccess) subTopic.ThumbnailPath = thumbResult.Data;
        }

        _unitOfWork.SubTopics.Update(subTopic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Alt başlık başarıyla güncellendi.");
    }
}
