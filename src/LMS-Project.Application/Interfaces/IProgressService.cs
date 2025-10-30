using LMS_Project.Application.DTOs;
using LMS_Project.Domain.Common;

namespace LMS_Project.Application.Interfaces;

public interface IProgressService
{
    Task<Result<UserProgressDto>> UpdateProgressAsync(
        Guid userId,
        Guid subTopicId,
        int durationSeconds,
        string sessionId,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default);

    Task<Result<SubTopicDto>> GetNextAvailableSubTopicAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default);

    Task<Result<ProgressSummaryDto>> GetModuleProgressSummaryAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> CanAccessSubTopicAsync(
        Guid userId,
        Guid subTopicId,
        CancellationToken cancellationToken = default);

    Task<Result> CompleteSubTopicAsync(
        Guid userId,
        Guid subTopicId,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> IsTrainingCompletedAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> IsModuleCompletedAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken cancellationToken = default);

    Task<Result> CheckAndCompleteTrainingAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default);

    Task<Result> CheckAndCompleteModuleAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken cancellationToken = default);
}
