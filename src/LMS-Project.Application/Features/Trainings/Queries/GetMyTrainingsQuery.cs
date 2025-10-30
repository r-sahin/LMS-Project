using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Trainings.Queries;

public record GetMyTrainingsQuery : IRequest<Result<List<MyTrainingDto>>>;

public class GetMyTrainingsQueryHandler : IRequestHandler<GetMyTrainingsQuery, Result<List<MyTrainingDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyTrainingsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MyTrainingDto>>> Handle(
        GetMyTrainingsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // Kullanıcının tüm UserTraining kayıtlarını getir
        var userTrainings = await _unitOfWork.UserTrainings.GetTrainingsByUserIdAsync(userId, cancellationToken);

        if (!userTrainings.Any())
        {
            return Result<List<MyTrainingDto>>.Success(
                new List<MyTrainingDto>(),
                "Size atanmış hiçbir eğitim bulunamadı.");
        }

        var trainingDtos = new List<MyTrainingDto>();

        foreach (var userTraining in userTrainings)
        {
            var training = await _unitOfWork.Trainings.GetTrainingWithSubTopicsAsync(userTraining.TrainingId, cancellationToken);
            if (training == null || !training.IsActive || !training.IsPublished)
                continue;

            var module = await _unitOfWork.Modules.GetByIdAsync(training.ModuleId, cancellationToken);

            // Alt başlıkların tamamlanma durumunu hesapla
            var totalSubTopics = training.SubTopics.Count;
            var completedSubTopics = await _unitOfWork.UserProgresses
                .GetCompletedSubTopicsCountByTrainingAsync(userId, training.Id, cancellationToken);

            // Kullanıcının bu eğitimde harcadığı toplam süreyi hesapla
            var totalStudyTime = 0;
            foreach (var subTopic in training.SubTopics)
            {
                var progress = await _unitOfWork.UserProgresses
                    .GetProgressByUserAndSubTopicAsync(userId, subTopic.Id, cancellationToken);
                if (progress != null)
                {
                    totalStudyTime += progress.DurationSeconds;
                }
            }

            // Sıradaki erişilebilir SubTopic'i bul
            Guid? nextSubTopicId = null;
            string? nextSubTopicName = null;

            foreach (var subTopic in training.SubTopics.OrderBy(st => st.OrderIndex))
            {
                var progress = await _unitOfWork.UserProgresses
                    .GetProgressByUserAndSubTopicAsync(userId, subTopic.Id, cancellationToken);

                if (progress == null || !progress.IsCompleted)
                {
                    // Sıralı öğrenme kontrolü: Erişebilir mi?
                    if (subTopic.OrderIndex == 0)
                    {
                        // İlk SubTopic her zaman erişilebilir
                        nextSubTopicId = subTopic.Id;
                        nextSubTopicName = subTopic.Name;
                        break;
                    }
                    else
                    {
                        // Önceki SubTopic tamamlandı mı?
                        var hasPreviousCompleted = await _unitOfWork.UserProgresses
                            .HasUserCompletedPreviousSubTopicAsync(userId, training.Id, subTopic.OrderIndex, cancellationToken);

                        if (hasPreviousCompleted)
                        {
                            nextSubTopicId = subTopic.Id;
                            nextSubTopicName = subTopic.Name;
                            break;
                        }
                    }
                }
            }

            trainingDtos.Add(new MyTrainingDto
            {
                Id = training.Id,
                ModuleId = training.ModuleId,
                ModuleName = module?.Name ?? "Bilinmeyen Modül",
                Name = training.Name,
                Description = training.Description,
                TotalDurationSeconds = training.TotalDurationSeconds,
                OrderIndex = training.OrderIndex,
                ThumbnailPath = training.ThumbnailPath,
                VideoIntroPath = training.VideoIntroPath,
                TotalSubTopics = totalSubTopics,
                CompletedSubTopics = completedSubTopics,
                CompletionPercentage = userTraining.CompletionPercentage,
                IsCompleted = userTraining.IsCompleted,
                CompletedDate = userTraining.CompletedDate,
                StartedDate = userTraining.StartedDate,
                TotalStudyTimeSeconds = totalStudyTime,
                NextAvailableSubTopicId = nextSubTopicId,
                NextAvailableSubTopicName = nextSubTopicName
            });
        }

        // OrderIndex'e göre sırala
        var sortedTrainings = trainingDtos.OrderBy(t => t.ModuleName).ThenBy(t => t.OrderIndex).ToList();

        return Result<List<MyTrainingDto>>.Success(
            sortedTrainings,
            $"{sortedTrainings.Count} eğitim bulundu.");
    }
}
