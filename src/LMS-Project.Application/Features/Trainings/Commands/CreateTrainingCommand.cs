using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS_Project.Application.Features.Trainings.Commands;

public record CreateTrainingCommand(
    Guid ModuleId,
    string Name,
    string? Description,
    int TotalDurationSeconds,
    IFormFile? ThumbnailFile,
    IFormFile? VideoIntroFile) : IRequest<Result<Guid>>;

public class CreateTrainingCommandValidator : AbstractValidator<CreateTrainingCommand>
{
    public CreateTrainingCommandValidator()
    {
        RuleFor(x => x.ModuleId).NotEmpty().WithMessage("Modül seçilmelidir.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TotalDurationSeconds).GreaterThan(0);
    }
}

public class CreateTrainingCommandHandler : IRequestHandler<CreateTrainingCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public CreateTrainingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result<Guid>> Handle(CreateTrainingCommand request, CancellationToken cancellationToken)
    {
        // Modülü kontrol et
        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module == null)
        {
            return Result<Guid>.Failure("Modül bulunamadı.");
        }

        // Mevcut eğitimlerin toplam süresini hesapla
        var trainings = await _unitOfWork.Trainings.GetTrainingsByModuleIdAsync(request.ModuleId, cancellationToken);
        var currentTotalSeconds = trainings.Sum(t => t.TotalDurationSeconds);

        // Modül süresi (dakika -> saniye)
        var moduleTotalSeconds = module.EstimatedDurationMinutes * 60;

        // Yeni eğitim eklenince toplam süre kontrol
        var newTotalSeconds = currentTotalSeconds + request.TotalDurationSeconds;

        if (newTotalSeconds > moduleTotalSeconds)
        {
            var remainingSeconds = moduleTotalSeconds - currentTotalSeconds;
            var remainingMinutes = remainingSeconds / 60;
            var remainingSecondsRemainder = remainingSeconds % 60;

            return Result<Guid>.Failure(
                $"Eğitim süresi modül süresini aşıyor! " +
                $"Modül toplam süresi: {module.EstimatedDurationMinutes} dakika ({moduleTotalSeconds} saniye). " +
                $"Mevcut eğitimler toplamı: {currentTotalSeconds} saniye. " +
                $"Kalan süre: {remainingMinutes} dakika {remainingSecondsRemainder} saniye. " +
                $"Girdiğiniz süre: {request.TotalDurationSeconds} saniye.");
        }

        var maxOrderIndex = trainings.Any() ? trainings.Max(t => t.OrderIndex) : -1;

        string? thumbnailPath = null, videoPath = null;

        if (request.ThumbnailFile != null)
        {
            var result = await _fileService.UploadFileAsync(request.ThumbnailFile, "images/trainings", cancellationToken);
            if (result.IsSuccess) thumbnailPath = result.Data;
        }

        if (request.VideoIntroFile != null)
        {
            var result = await _fileService.UploadFileAsync(request.VideoIntroFile, "videos/trainings", cancellationToken);
            if (result.IsSuccess) videoPath = result.Data;
        }

        var training = new Training
        {
            ModuleId = request.ModuleId,
            Name = request.Name,
            Description = request.Description,
            TotalDurationSeconds = request.TotalDurationSeconds,
            OrderIndex = maxOrderIndex + 1,
            ThumbnailPath = thumbnailPath,
            VideoIntroPath = videoPath,
            IsActive = true,
            CreatedBy = _currentUserService.UserId.ToString()
        };

        await _unitOfWork.Trainings.AddAsync(training, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(training.Id, "Eğitim başarıyla oluşturuldu.");
    }
}
