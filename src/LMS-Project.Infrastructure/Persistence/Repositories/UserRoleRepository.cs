using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(ApplicationDbContext context) : base(context) { }
    public async Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => 
        await _dbSet.Include(ur => ur.Role).Where(ur => !ur.IsDeleted && ur.UserId == userId).ToListAsync(cancellationToken);
    public async Task<IEnumerable<UserRole>> GetUserRolesByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default) => 
        await _dbSet.Include(ur => ur.User).Where(ur => !ur.IsDeleted && ur.RoleId == roleId).ToListAsync(cancellationToken);
    public async Task<bool> HasUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default) => 
        await _dbSet.Where(ur => !ur.IsDeleted).AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
}
