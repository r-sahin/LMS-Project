using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext context) : base(context) { }
    public async Task<Permission?> GetByNameAsync(string permissionName, CancellationToken cancellationToken = default) => 
        await _dbSet.Where(p => !p.IsDeleted).FirstOrDefaultAsync(p => p.Name == permissionName, cancellationToken);
    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default) => 
        await _context.Set<RolePermission>().Include(rp => rp.Permission).Where(rp => !rp.IsDeleted && rp.RoleId == roleId).Select(rp => rp.Permission).ToListAsync(cancellationToken);
    public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => 
        await _context.Set<UserRole>().Include(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission).Where(ur => !ur.IsDeleted && ur.UserId == userId).SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission)).Distinct().ToListAsync(cancellationToken);
}
