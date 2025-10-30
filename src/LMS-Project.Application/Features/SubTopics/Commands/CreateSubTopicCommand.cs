using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS_Project.Application.Features.SubTopics.Commands;

public record CreateSubTopicCommand(
    Guid TrainingId,
    string Name,
    string? Description,
    int MinimumDurationSeconds,
    bool IsMandatory,
    IFormFile ZipFile,
    IFormFile? ThumbnailFile) : IRequest<Result<Guid>>;

public class CreateSubTopicCommandValidator : AbstractValidator<CreateSubTopicCommand>
{
    public CreateSubTopicCommandValidator()
    {
        RuleFor(x => x.TrainingId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MinimumDurationSeconds).GreaterThan(0).WithMessage("Minimum süre 0'dan büyük olmalıdır.");
        RuleFor(x => x.ZipFile).NotNull().WithMessage("ZIP dosyası gereklidir.");
    }
}

public class CreateSubTopicCommandHandler : IRequestHandler<CreateSubTopicCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public CreateSubTopicCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result<Guid>> Handle(CreateSubTopicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var training = await _unitOfWork.Trainings.GetByIdAsync(request.TrainingId, cancellationToken);
            if (training == null) return Result<Guid>.Failure("Eğitim bulunamadı.");

        
        var subTopics = await _unitOfWork.SubTopics.GetSubTopicsByTrainingIdAsync(request.TrainingId, cancellationToken);
        var currentTotalSeconds = subTopics.Sum(st => st.MinimumDurationSeconds);
        var newTotalSeconds = currentTotalSeconds + request.MinimumDurationSeconds;

        if (newTotalSeconds > training.TotalDurationSeconds)
        {
            var remainingSeconds = training.TotalDurationSeconds - currentTotalSeconds;
            var remainingMinutes = remainingSeconds / 60;
            var remainingSecondsRemainder = remainingSeconds % 60;

            return Result<Guid>.Failure(
                $"Alt başlık süresi eğitim süresini aşıyor! " +
                $"Eğitim toplam süresi: {training.TotalDurationSeconds} saniye. " +
                $"Mevcut alt başlıklar toplamı: {currentTotalSeconds} saniye. " +
                $"Kalan süre: {remainingMinutes} dakika {remainingSecondsRemainder} saniye. " +
                $"Girdiğiniz süre: {request.MinimumDurationSeconds} saniye.");
        }

        var maxOrderIndex = subTopics.Any() ? subTopics.Max(st => st.OrderIndex) : -1;

        var subTopic = new SubTopic
        {
            TrainingId = request.TrainingId,
            Name = request.Name,
            Description = request.Description,
            MinimumDurationSeconds = request.MinimumDurationSeconds,
            IsMandatory = request.IsMandatory,
            OrderIndex = maxOrderIndex + 1,
            IsActive = true,
            ZipFilePath = "",
            HtmlFilePath = "",
            CreatedBy = _currentUserService.UserId.ToString()
        };

        await _unitOfWork.SubTopics.AddAsync(subTopic, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ZIP yükle ve ayıkla
        var zipResult = await _fileService.UploadAndExtractZipAsync(
            request.ZipFile,
            training.ModuleId,
            request.TrainingId,
            subTopic.Id,
            cancellationToken);

        if (!zipResult.IsSuccess)
        {
            // Hata varsa SubTopic'i sil
            _unitOfWork.SubTopics.Remove(subTopic);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var errorMessage = string.IsNullOrWhiteSpace(zipResult.Message)
                ? "ZIP dosyası yüklenirken bilinmeyen bir hata oluştu."
                : zipResult.Message;
            return Result<Guid>.Failure(errorMessage);
        }

        subTopic.HtmlFilePath = zipResult.Data!;
        subTopic.ZipFilePath = $"/content/modules/{training.ModuleId}/trainings/{request.TrainingId}/subtopics/{subTopic.Id}/content.zip";

        // Thumbnail
        if (request.ThumbnailFile != null)
        {
            var thumbResult = await _fileService.UploadFileAsync(request.ThumbnailFile, "images/subtopics", cancellationToken);
            if (thumbResult.IsSuccess) subTopic.ThumbnailPath = thumbResult.Data;
        }

            _unitOfWork.SubTopics.Update(subTopic);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var successMessage = $"Alt başlık başarıyla oluşturuldu! ZIP dosyası ayıklandı ve {subTopic.Name} eğitime eklendi.";
            return Result<Guid>.Success(subTopic.Id, successMessage);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Alt başlık oluşturulurken hata oluştu: {ex.Message}");
        }
    }
}
