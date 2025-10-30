using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context) { }
    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default) => 
        await _dbSet.Where(r => !r.IsDeleted).FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
    public async Task<Role?> GetRoleWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default) => 
        await _dbSet.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission).Where(r => !r.IsDeleted).FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => 
        await _context.Set<UserRole>().Include(ur => ur.Role).Where(ur => !ur.IsDeleted && ur.UserId == userId).Select(ur => ur.Role).ToListAsync(cancellationToken);
}
