using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface IUserTrainingRepository : IRepository<UserTraining>
{
    Task<UserTraining?> GetByUserAndTrainingAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserTraining>> GetTrainingsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserTraining>> GetCompletedTrainingsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserTraining>> GetTrainingsByUserAndModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default);
}
