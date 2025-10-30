using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface IUserProgressRepository : IRepository<UserProgress>
{
    Task<UserProgress?> GetProgressByUserAndSubTopicAsync(Guid userId, Guid subTopicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserProgress>> GetProgressesByUserAndTrainingAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserProgress>> GetProgressesByUserAndModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default);
    Task<int> GetCompletedSubTopicsCountByTrainingAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default);
    Task<bool> HasUserCompletedPreviousSubTopicAsync(Guid userId, Guid trainingId, int orderIndex, CancellationToken cancellationToken = default);
}
