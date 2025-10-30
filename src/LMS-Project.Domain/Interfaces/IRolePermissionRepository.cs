using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface IRolePermissionRepository : IRepository<RolePermission>
{
    Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RolePermission>> GetRolePermissionsByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
    Task<bool> HasRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
}
