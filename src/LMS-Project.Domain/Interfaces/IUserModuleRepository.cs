using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface IUserModuleRepository : IRepository<UserModule>
{
    Task<UserModule?> GetByUserAndModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserModule>> GetModulesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserModule>> GetCompletedModulesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
