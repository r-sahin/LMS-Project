using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Trainings.Queries;

public record GetTrainingByIdQuery(Guid TrainingId) : IRequest<Result<TrainingDto>>;

public class GetTrainingByIdQueryHandler : IRequestHandler<GetTrainingByIdQuery, Result<TrainingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProgressService _progressService;

    public GetTrainingByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IProgressService progressService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _progressService = progressService;
    }

    public async Task<Result<TrainingDto>> Handle(
        GetTrainingByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // ⭐ 1. ÖNCE EĞİTİMİN VAR OLUP OLMADIĞINI KONTROL ET
        var training = await _unitOfWork.Trainings
            .GetTrainingWithSubTopicsAsync(request.TrainingId, cancellationToken);

        if (training == null)
        {
            return Result<TrainingDto>.Failure("Eğitim bulunamadı.");
        }

        // ⭐ 2. EĞİTİM YAYINLANMIŞ MI KONTROL ET
        if (!training.IsPublished)
        {
            return Result<TrainingDto>.Failure("Bu eğitim henüz yayınlanmamış.");
        }

        // ⭐ 3. MODÜL YAYINLANMIŞ MI KONTROL ET
        var module = await _unitOfWork.Modules.GetByIdAsync(training.ModuleId, cancellationToken);
        if (module == null)
        {
            return Result<TrainingDto>.Failure("Eğitime ait modül bulunamadı.");
        }

        if (!module.IsPublished)
        {
            return Result<TrainingDto>.Failure("Bu eğitime ait modül henüz yayınlanmamış.");
        }

        // ⭐ 4. KULLANICININ MODÜLE ERİŞİMİ VAR MI KONTROL ET
        var userModule = await _unitOfWork.UserModules.GetByUserAndModuleAsync(
            userId, training.ModuleId, cancellationToken);

        if (userModule == null)
        {
            return Result<TrainingDto>.Failure("Bu modüle erişim yetkiniz yok. Önce modül için talepte bulunmalısınız.");
        }

        // ⭐ 5. KULLANICININ EĞİTİME ERİŞİMİ VAR MI KONTROL ET (UserTraining)
        var userTraining = await _unitOfWork.UserTrainings
            .GetByUserAndTrainingAsync(userId, training.Id, cancellationToken);

        if (userTraining == null)
        {
            // Bekleyen talep var mı kontrol et
            var hasPendingRequest = await _unitOfWork.TrainingRequests
                .HasPendingRequestAsync(userId, training.Id, cancellationToken);

            if (hasPendingRequest)
            {
                return Result<TrainingDto>.Failure("Bu eğitime erişim talebiniz beklemede. Moderator onayını bekleyin.");
            }
            else
            {
                return Result<TrainingDto>.Failure("Bu eğitime erişim yetkiniz yok. Lütfen eğitim için talepte bulunun.");
            }
        }

        var subTopicDtos = new List<SubTopicDto>();

        foreach (var subTopic in training.SubTopics.OrderBy(st => st.OrderIndex))
        {
            var userProgress = await _unitOfWork.UserProgresses
                .GetProgressByUserAndSubTopicAsync(userId, subTopic.Id, cancellationToken);

            var canAccessResult = await _progressService.CanAccessSubTopicAsync(
                userId, subTopic.Id, cancellationToken);

            subTopicDtos.Add(new SubTopicDto
            {
                Id = subTopic.Id,
                TrainingId = subTopic.TrainingId,
                Name = subTopic.Name,
                Description = subTopic.Description,
                MinimumDurationSeconds = subTopic.MinimumDurationSeconds,
                ZipFilePath = subTopic.ZipFilePath,
                HtmlFilePath = subTopic.HtmlFilePath,
                OrderIndex = subTopic.OrderIndex,
                IsActive = subTopic.IsActive,
                IsMandatory = subTopic.IsMandatory,
                ThumbnailPath = subTopic.ThumbnailPath,
                IsCompleted = userProgress?.IsCompleted ?? false,
                IsLocked = userProgress?.IsLocked ?? !canAccessResult.IsSuccess,
                CurrentDurationSeconds = userProgress?.DurationSeconds ?? 0,
                CompletedDate = userProgress?.CompletedDate,
                LastAccessedDate = userProgress?.LastAccessedDate
            });
        }

        var trainingDto = new TrainingDto
        {
            Id = training.Id,
            ModuleId = training.ModuleId,
            ModuleName = training.Module.Name,
            Name = training.Name,
            Description = training.Description,
            TotalDurationSeconds = training.TotalDurationSeconds,
            OrderIndex = training.OrderIndex,
            IsActive = training.IsActive,
            IsCompleted = userTraining?.IsCompleted ?? false,
            ThumbnailPath = training.ThumbnailPath,
            VideoIntroPath = training.VideoIntroPath,
            TotalSubTopics = training.SubTopics.Count,
            CompletedSubTopics = subTopicDtos.Count(st => st.IsCompleted),
            CompletionPercentage = userTraining?.CompletionPercentage ?? 0,
            SubTopics = subTopicDtos
        };

        return Result<TrainingDto>.Success(trainingDto, "Eğitim başarıyla getirildi.");
    }
}
