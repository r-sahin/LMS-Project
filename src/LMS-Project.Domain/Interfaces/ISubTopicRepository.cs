using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface ISubTopicRepository : IRepository<SubTopic>
{
    Task<IEnumerable<SubTopic>> GetSubTopicsByTrainingIdAsync(Guid trainingId, CancellationToken cancellationToken = default);
    Task<SubTopic?> GetNextSubTopicAsync(Guid trainingId, int currentOrderIndex, CancellationToken cancellationToken = default);
    Task<SubTopic?> GetFirstSubTopicOfTrainingAsync(Guid trainingId, CancellationToken cancellationToken = default);
    Task<bool> IsSubTopicCompletedByUserAsync(Guid userId, Guid subTopicId, CancellationToken cancellationToken = default);
}
