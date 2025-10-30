using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class RolePermissionRepository : Repository<RolePermission>, IRolePermissionRepository
{
    public RolePermissionRepository(ApplicationDbContext context) : base(context) { }
    public async Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default) => 
        await _dbSet.Include(rp => rp.Permission).Where(rp => !rp.IsDeleted && rp.RoleId == roleId).ToListAsync(cancellationToken);
    public async Task<IEnumerable<RolePermission>> GetRolePermissionsByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default) => 
        await _dbSet.Include(rp => rp.Role).Where(rp => !rp.IsDeleted && rp.PermissionId == permissionId).ToListAsync(cancellationToken);
    public async Task<bool> HasRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default) => 
        await _dbSet.Where(rp => !rp.IsDeleted).AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
}
