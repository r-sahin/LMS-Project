using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface IUserRoleRepository : IRepository<UserRole>
{
    Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRole>> GetUserRolesByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> HasUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}
