using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class ModuleRepository : Repository<Module>, IModuleRepository
{
    public ModuleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Module?> GetModuleWithTrainingsAsync(
        Guid moduleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.Trainings.Where(t => !t.IsDeleted))
                .ThenInclude(t => t.SubTopics.Where(st => !st.IsDeleted))
            .Where(m => !m.IsDeleted)
            .FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken);
    }

    public async Task<IEnumerable<Module>> GetActiveModulesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.Trainings.Where(t => !t.IsDeleted && t.IsActive))
            .Where(m => !m.IsDeleted && m.IsActive)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Module>> GetModulesWithTrainingsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.Trainings.Where(t => !t.IsDeleted))
                .ThenInclude(t => t.SubTopics.Where(st => !st.IsDeleted))
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsModuleCompletedByUserAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken cancellationToken = default)
    {
        var userModule = await _context.Set<UserModule>()
            .Where(um => !um.IsDeleted)
            .FirstOrDefaultAsync(
                um => um.UserId == userId && um.ModuleId == moduleId,
                cancellationToken);

        return userModule?.IsCompleted ?? false;
    }
}
