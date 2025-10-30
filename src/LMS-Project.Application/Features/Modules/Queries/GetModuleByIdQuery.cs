using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Modules.Queries;

public record GetModuleByIdQuery(Guid ModuleId) : IRequest<Result<ModuleDto>>;

public class GetModuleByIdQueryHandler : IRequestHandler<GetModuleByIdQuery, Result<ModuleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetModuleByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ModuleDto>> Handle(
        GetModuleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // ⭐ 1. ÖNCE MODÜLÜN VAR OLUP OLMADIĞINI KONTROL ET
        var module = await _unitOfWork.Modules
            .GetModuleWithTrainingsAsync(request.ModuleId, cancellationToken);

        if (module == null)
        {
            return Result<ModuleDto>.Failure("Modül bulunamadı.");
        }

        // ⭐ 2. MODÜL VARSA YETKİ KONTROLÜ YAP
        var userModule = await _unitOfWork.UserModules
            .GetByUserAndModuleAsync(userId, request.ModuleId, cancellationToken);

        if (userModule == null)
        {
            return Result<ModuleDto>.Failure("Bu modüle erişim yetkiniz yok. Lütfen kayıt talebinde bulunun.");
        }

        var trainingDtos = new List<TrainingDto>();

        // ⚠️ ÖNEMLİ: Sadece yayınlanmış eğitimleri göster
        foreach (var training in module.Trainings.Where(t => t.IsPublished).OrderBy(t => t.OrderIndex))
        {
            var userTraining = await _unitOfWork.UserTrainings
                .GetByUserAndTrainingAsync(userId, training.Id, cancellationToken);

            // ⭐ Eğitime erişim kontrolü - UserTraining var mı?
            var isLocked = userTraining == null;

            // Bekleyen talep var mı?
            var hasPendingRequest = await _unitOfWork.TrainingRequests
                .HasPendingRequestAsync(userId, training.Id, cancellationToken);

            // Kilit nedeni belirleme
            string? lockReason = null;
            if (isLocked)
            {
                if (hasPendingRequest)
                {
                    lockReason = "Bu eğitime erişim talebiniz beklemede. Moderator onayı bekleniyor.";
                }
                else
                {
                    lockReason = "Bu eğitime erişim için talep oluşturmalısınız.";
                }
            }

            trainingDtos.Add(new TrainingDto
            {
                Id = training.Id,
                ModuleId = training.ModuleId,
                ModuleName = module.Name,
                Name = training.Name,
                Description = training.Description,
                TotalDurationSeconds = training.TotalDurationSeconds,
                OrderIndex = training.OrderIndex,
                IsActive = training.IsActive,
                IsCompleted = userTraining?.IsCompleted ?? false,
                ThumbnailPath = training.ThumbnailPath,
                VideoIntroPath = training.VideoIntroPath,
                TotalSubTopics = training.SubTopics.Count,
                CompletedSubTopics = 0,
                CompletionPercentage = userTraining?.CompletionPercentage ?? 0,
                IsLocked = isLocked,
                HasPendingRequest = hasPendingRequest,
                LockReason = lockReason
            });
        }

        var moduleDto = new ModuleDto
        {
            Id = module.Id,
            Name = module.Name,
            Description = module.Description,
            OrderIndex = module.OrderIndex,
            IsActive = module.IsActive,
            ImagePath = module.ImagePath,
            EstimatedDurationMinutes = module.EstimatedDurationMinutes,
            TotalTrainings = module.Trainings.Count,
            CompletedTrainings = trainingDtos.Count(t => t.IsCompleted),
            CompletionPercentage = userModule?.CompletionPercentage ?? 0,
            Trainings = trainingDtos
        };

        return Result<ModuleDto>.Success(moduleDto, "Modül başarıyla getirildi.");
    }
}
