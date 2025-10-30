using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface ITrainingRepository : IRepository<Training>
{
    Task<Training?> GetTrainingWithSubTopicsAsync(Guid trainingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Training>> GetTrainingsByModuleIdAsync(Guid moduleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Training>> GetActiveTrainingsByModuleIdAsync(Guid moduleId, CancellationToken cancellationToken = default);
    Task<bool> IsTrainingCompletedByUserAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default);
}
