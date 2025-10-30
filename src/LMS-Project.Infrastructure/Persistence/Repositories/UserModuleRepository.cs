using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class UserModuleRepository : Repository<UserModule>, IUserModuleRepository
{
    public UserModuleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserModule?> GetByUserAndModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(um => um.Module).Where(um => !um.IsDeleted)
            .FirstOrDefaultAsync(um => um.UserId == userId && um.ModuleId == moduleId, cancellationToken);
    }

    public async Task<IEnumerable<UserModule>> GetModulesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(um => um.Module).Where(um => !um.IsDeleted && um.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserModule>> GetCompletedModulesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(um => um.Module).Where(um => !um.IsDeleted && um.UserId == userId && um.IsCompleted).ToListAsync(cancellationToken);
    }
}
