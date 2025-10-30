using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface ITrainingRequestRepository : IRepository<TrainingRequest>
{
    Task<IEnumerable<TrainingRequest>> GetRequestsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrainingRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    Task<bool> HasPendingRequestAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default);
    Task<TrainingRequest?> GetUserRequestForTrainingAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default);
}
