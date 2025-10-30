using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<User?> GetUserWithRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetUserWithPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<bool> IsEmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsUserNameExistsAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .AnyAsync(u => u.UserName == userName, cancellationToken);
    }
}
