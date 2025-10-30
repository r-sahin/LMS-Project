using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;

namespace LMS_Project.Infrastructure.Services;

public class ProgressService : IProgressService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProgressService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserProgressDto>> UpdateProgressAsync(
        Guid userId,
        Guid subTopicId,
        int durationSeconds,
        string sessionId,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Alt başlığı getir
        var subTopic = await _unitOfWork.SubTopics.GetByIdAsync(subTopicId, cancellationToken);
        if (subTopic == null)
        {
            return Result<UserProgressDto>.Failure("Alt başlık bulunamadı.");
        }

        // 2. Eğitim erişim kontrolü - UserTraining var mı?
        var userTraining = await _unitOfWork.UserTrainings
            .GetByUserAndTrainingAsync(userId, subTopic.TrainingId, cancellationToken);

        if (userTraining == null)
        {
            return Result<UserProgressDto>.Failure("Bu eğitime erişim yetkiniz yok. Lütfen eğitim için talepte bulunun.");
        }

        // 3. ⭐ TEK CİHAZ KONTROLÜ - Aynı Training'de başka aktif session var mı?
        var allProgressesInTraining = await _unitOfWork.UserProgresses
            .GetProgressesByUserAndTrainingAsync(userId, subTopic.TrainingId, cancellationToken);

        var otherActiveSessions = allProgressesInTraining
            .Where(p => p.IsSessionActive &&
                       p.SessionId != null &&
                       p.SessionId != sessionId &&
                       p.LastAccessedDate > DateTime.UtcNow.AddMinutes(-5)) // Son 5 dakikada aktif
            .ToList();

        if (otherActiveSessions.Any())
        {
            // Diğer session'ları sonlandır
            foreach (var otherSession in otherActiveSessions)
            {
                otherSession.IsSessionActive = false;
                otherSession.UpdatedBy = userId.ToString();
                otherSession.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.UserProgresses.Update(otherSession);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Frontend'e bilgi dön (diğer cihazdan logout oldu)
            return Result<UserProgressDto>.Failure("Başka bir cihazdan giriş yapıldı. Önceki oturum sonlandırıldı.");
        }

        // 4. Alt başlığa erişim kontrolü (sıralı öğrenme)
        var canAccessResult = await CanAccessSubTopicAsync(userId, subTopicId, cancellationToken);
        if (!canAccessResult.IsSuccess)
        {
            return Result<UserProgressDto>.Failure(canAccessResult.Message);
        }

        // 3. Kullanıcının mevcut progress kaydını getir veya oluştur
        var userProgress = await _unitOfWork.UserProgresses
            .GetProgressByUserAndSubTopicAsync(userId, subTopicId, cancellationToken);

        if (userProgress == null)
        {
            userProgress = new UserProgress
            {
                UserId = userId,
                SubTopicId = subTopicId,
                DurationSeconds = durationSeconds,
                LastAccessedDate = DateTime.UtcNow,
                AccessCount = 1,
                SessionId = sessionId,
                IpAddress = ipAddress,
                DeviceInfo = deviceInfo,
                IsSessionActive = true,
                CreatedBy = userId.ToString()
            };
            await _unitOfWork.UserProgresses.AddAsync(userProgress, cancellationToken);

            // ⚠️ Yeni oluşturulan progress'i kaydet ki CompleteSubTopicAsync içinde bulunabilsin
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Eğer zaten tamamlanmışsa süreyi güncellemeden devam et
            if (!userProgress.IsCompleted)
            {
                userProgress.DurationSeconds += durationSeconds;
                userProgress.LastAccessedDate = DateTime.UtcNow;
                userProgress.AccessCount++;
                userProgress.SessionId = sessionId; // ⭐ Session güncelle
                userProgress.IpAddress = ipAddress;
                userProgress.DeviceInfo = deviceInfo;
                userProgress.IsSessionActive = true;
                userProgress.UpdatedBy = userId.ToString();
                userProgress.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.UserProgresses.Update(userProgress);
            }
        }

        // 4. Minimum süre kontrolü ve tamamlanma durumu
        if (!userProgress.IsCompleted && userProgress.DurationSeconds >= subTopic.MinimumDurationSeconds)
        {
            await CompleteSubTopicAsync(userId, subTopicId, cancellationToken);

            // Progress'i yeniden getir
            userProgress = await _unitOfWork.UserProgresses
                .GetProgressByUserAndSubTopicAsync(userId, subTopicId, cancellationToken);

            // ⚠️ Eğer hala null ise hata ver
            if (userProgress == null)
            {
                return Result<UserProgressDto>.Failure("İlerleme kaydı güncellenemedi.");
            }
        }
        else
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // 5. DTO'ya dönüştür ve döndür
        var progressDto = new UserProgressDto
        {
            Id = userProgress.Id,
            UserId = userProgress.UserId,
            SubTopicId = userProgress.SubTopicId,
            SubTopicName = subTopic.Name,
            DurationSeconds = userProgress.DurationSeconds,
            MinimumDurationSeconds = subTopic.MinimumDurationSeconds,
            IsCompleted = userProgress.IsCompleted,
            CompletedDate = userProgress.CompletedDate,
            LastAccessedDate = userProgress.LastAccessedDate,
            AccessCount = userProgress.AccessCount,
            IsLocked = userProgress.IsLocked,
            CompletionPercentage = Math.Min(100, (decimal)userProgress.DurationSeconds / subTopic.MinimumDurationSeconds * 100)
        };

        return Result<UserProgressDto>.Success(progressDto, "İlerleme kaydedildi.");
    }

    public async Task<Result<SubTopicDto>> GetNextAvailableSubTopicAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        var subTopics = await _unitOfWork.SubTopics.GetSubTopicsByTrainingIdAsync(trainingId, cancellationToken);

        foreach (var subTopic in subTopics.OrderBy(st => st.OrderIndex))
        {
            var userProgress = await _unitOfWork.UserProgresses
                .GetProgressByUserAndSubTopicAsync(userId, subTopic.Id, cancellationToken);

            // Tamamlanmamış ilk alt başlığı bul
            if (userProgress == null || !userProgress.IsCompleted)
            {
                // Erişim kontrolü yap
                var canAccess = await CanAccessSubTopicAsync(userId, subTopic.Id, cancellationToken);

                if (canAccess.IsSuccess)
                {
                    return Result<SubTopicDto>.Success(new SubTopicDto
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
                        IsCompleted = false,
                        IsLocked = false,
                        CurrentDurationSeconds = userProgress?.DurationSeconds ?? 0,
                        CompletedDate = null,
                        LastAccessedDate = userProgress?.LastAccessedDate
                    });
                }
            }
        }

        return Result<SubTopicDto>.Failure("Tüm alt başlıklar tamamlanmış veya erişilebilir alt başlık yok.");
    }

    public async Task<Result<ProgressSummaryDto>> GetModuleProgressSummaryAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken cancellationToken = default)
    {
        var module = await _unitOfWork.Modules.GetModuleWithTrainingsAsync(moduleId, cancellationToken);
        if (module == null)
        {
            return Result<ProgressSummaryDto>.Failure("Modül bulunamadı.");
        }

        var trainingProgressList = new List<TrainingProgressDto>();
        int totalSubTopics = 0;
        int completedSubTopics = 0;
        int completedTrainings = 0;

        foreach (var training in module.Trainings.OrderBy(t => t.OrderIndex))
        {
            var trainingSubTopicsCount = training.SubTopics.Count;
            var trainingCompletedCount = await _unitOfWork.UserProgresses
                .GetCompletedSubTopicsCountByTrainingAsync(userId, training.Id, cancellationToken);

            totalSubTopics += trainingSubTopicsCount;
            completedSubTopics += trainingCompletedCount;

            bool isTrainingCompleted = trainingSubTopicsCount > 0 && trainingCompletedCount == trainingSubTopicsCount;
            if (isTrainingCompleted) completedTrainings++;

            trainingProgressList.Add(new TrainingProgressDto
            {
                TrainingId = training.Id,
                TrainingName = training.Name,
                TotalSubTopics = trainingSubTopicsCount,
                CompletedSubTopics = trainingCompletedCount,
                CompletionPercentage = trainingSubTopicsCount > 0
                    ? (decimal)trainingCompletedCount / trainingSubTopicsCount * 100
                    : 0,
                IsCompleted = isTrainingCompleted
            });
        }

        var summary = new ProgressSummaryDto
        {
            ModuleId = module.Id,
            ModuleName = module.Name,
            TotalTrainings = module.Trainings.Count,
            CompletedTrainings = completedTrainings,
            TotalSubTopics = totalSubTopics,
            CompletedSubTopics = completedSubTopics,
            ModuleCompletionPercentage = totalSubTopics > 0
                ? (decimal)completedSubTopics / totalSubTopics * 100
                : 0,
            Trainings = trainingProgressList
        };

        return Result<ProgressSummaryDto>.Success(summary);
    }

    public async Task<Result<bool>> CanAccessSubTopicAsync(
        Guid userId,
        Guid subTopicId,
        CancellationToken cancellationToken = default)
    {
        var subTopic = await _unitOfWork.SubTopics.GetByIdAsync(subTopicId, cancellationToken);
        if (subTopic == null)
        {
            return Result<bool>.Failure("Alt başlık bulunamadı.");
        }

        // İlk alt başlık her zaman erişilebilir
        if (subTopic.OrderIndex == 0)
        {
            return Result<bool>.Success(true);
        }

        // Önceki alt başlığın tamamlanıp tamamlanmadığını kontrol et
        var hasPreviousCompleted = await _unitOfWork.UserProgresses
            .HasUserCompletedPreviousSubTopicAsync(userId, subTopic.TrainingId, subTopic.OrderIndex, cancellationToken);

        if (!hasPreviousCompleted)
        {
            return Result<bool>.Failure("Önceki alt başlığı tamamlamadan bu alt başlığa erişemezsiniz.");
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result> CompleteSubTopicAsync(
        Guid userId,
        Guid subTopicId,
        CancellationToken cancellationToken = default)
    {
        var userProgress = await _unitOfWork.UserProgresses
            .GetProgressByUserAndSubTopicAsync(userId, subTopicId, cancellationToken);

        if (userProgress == null)
        {
            return Result.Failure("İlerleme kaydı bulunamadı.");
        }

        if (userProgress.IsCompleted)
        {
            return Result.Success("Alt başlık zaten tamamlanmış.");
        }

        var subTopic = await _unitOfWork.SubTopics.GetByIdAsync(subTopicId, cancellationToken);
        if (subTopic == null)
        {
            return Result.Failure("Alt başlık bulunamadı.");
        }

        // Minimum süre kontrolü
        if (userProgress.DurationSeconds < subTopic.MinimumDurationSeconds)
        {
            return Result.Failure($"Minimum süre ({subTopic.MinimumDurationSeconds} saniye) tamamlanmadı.");
        }

        userProgress.IsCompleted = true;
        userProgress.CompletedDate = DateTime.UtcNow;
        userProgress.UpdatedBy = userId.ToString();
        userProgress.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.UserProgresses.Update(userProgress);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Training ve Module tamamlanma kontrolü yap
        await CheckAndCompleteTrainingAsync(userId, subTopic.TrainingId, cancellationToken);

        return Result.Success("Alt başlık başarıyla tamamlandı.");
    }

    public async Task<Result<bool>> IsTrainingCompletedAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        var training = await _unitOfWork.Trainings.GetTrainingWithSubTopicsAsync(trainingId, cancellationToken);
        if (training == null)
        {
            return Result<bool>.Failure("Eğitim bulunamadı.");
        }

        var totalSubTopics = training.SubTopics.Count;
        if (totalSubTopics == 0)
        {
            return Result<bool>.Success(false);
        }

        var completedCount = await _unitOfWork.UserProgresses
            .GetCompletedSubTopicsCountByTrainingAsync(userId, trainingId, cancellationToken);

        return Result<bool>.Success(completedCount == totalSubTopics);
    }

    public async Task<Result<bool>> IsModuleCompletedAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken cancellationToken = default)
    {
        var module = await _unitOfWork.Modules.GetModuleWithTrainingsAsync(moduleId, cancellationToken);
        if (module == null)
        {
            return Result<bool>.Failure("Modül bulunamadı.");
        }

        foreach (var training in module.Trainings)
        {
            var isCompleted = await IsTrainingCompletedAsync(userId, training.Id, cancellationToken);
            if (!isCompleted.IsSuccess || !isCompleted.Data)
            {
                return Result<bool>.Success(false);
            }
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result> CheckAndCompleteTrainingAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        var isCompletedResult = await IsTrainingCompletedAsync(userId, trainingId, cancellationToken);
        if (!isCompletedResult.IsSuccess || !isCompletedResult.Data)
        {
            return Result.Success("Eğitim henüz tamamlanmadı.");
        }

        var userTraining = await _unitOfWork.UserTrainings
            .GetByUserAndTrainingAsync(userId, trainingId, cancellationToken);

        // ⚠️ UserTraining yoksa hata ver - Artık otomatik oluşturmuyoruz (Training Request System)
        if (userTraining == null)
        {
            return Result.Failure("Bu eğitime erişim yetkiniz yok.");
        }

        userTraining.IsCompleted = true;
        userTraining.CompletedDate = DateTime.UtcNow;
        userTraining.CompletionPercentage = 100;
        userTraining.UpdatedBy = userId.ToString();
        userTraining.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.UserTrainings.Update(userTraining);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Eğitim tamamlandı, şimdi modül kontrolü yap
        var training = await _unitOfWork.Trainings.GetByIdAsync(trainingId, cancellationToken);
        if (training != null)
        {
            await CheckAndCompleteModuleAsync(userId, training.ModuleId, cancellationToken);
        }

        return Result.Success("Eğitim başarıyla tamamlandı.");
    }

    public async Task<Result> CheckAndCompleteModuleAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken cancellationToken = default)
    {
        var isCompletedResult = await IsModuleCompletedAsync(userId, moduleId, cancellationToken);
        if (!isCompletedResult.IsSuccess || !isCompletedResult.Data)
        {
            return Result.Success("Modül henüz tamamlanmadı.");
        }

        var userModule = await _unitOfWork.UserModules
            .GetByUserAndModuleAsync(userId, moduleId, cancellationToken);

        // ⚠️ UserModule yoksa hata ver - Artık otomatik oluşturmuyoruz (Module Request System)
        if (userModule == null)
        {
            return Result.Failure("Bu modüle erişim yetkiniz yok.");
        }

        if (!userModule.IsCompleted)
        {
            userModule.IsCompleted = true;
            userModule.CompletedDate = DateTime.UtcNow;
            userModule.CompletionPercentage = 100;
            userModule.UpdatedBy = userId.ToString();
            userModule.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.UserModules.Update(userModule);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success("Modül başarıyla tamamlandı.");
    }
}
