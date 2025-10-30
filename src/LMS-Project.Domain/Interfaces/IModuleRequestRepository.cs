using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface IModuleRequestRepository : IRepository<ModuleRequest>
{
    Task<IEnumerable<ModuleRequest>> GetRequestsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ModuleRequest>> GetRequestsByModuleIdAsync(Guid moduleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ModuleRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    Task<ModuleRequest?> GetUserRequestForModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingRequestAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default);
}
