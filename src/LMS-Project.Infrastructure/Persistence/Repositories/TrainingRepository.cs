using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class TrainingRepository : Repository<Training>, ITrainingRepository
{
    public TrainingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Training?> GetTrainingWithSubTopicsAsync(
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Module)
            .Include(t => t.SubTopics.Where(st => !st.IsDeleted))
            .Where(t => !t.IsDeleted)
            .FirstOrDefaultAsync(t => t.Id == trainingId, cancellationToken);
    }

    public async Task<IEnumerable<Training>> GetTrainingsByModuleIdAsync(
        Guid moduleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.SubTopics.Where(st => !st.IsDeleted))
            .Where(t => !t.IsDeleted)
            .Where(t => t.ModuleId == moduleId)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Training>> GetActiveTrainingsByModuleIdAsync(
        Guid moduleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.SubTopics.Where(st => !st.IsDeleted && st.IsActive))
            .Where(t => !t.IsDeleted && t.IsActive)
            .Where(t => t.ModuleId == moduleId)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsTrainingCompletedByUserAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        var userTraining = await _context.Set<UserTraining>()
            .Where(ut => !ut.IsDeleted)
            .FirstOrDefaultAsync(
                ut => ut.UserId == userId && ut.TrainingId == trainingId,
                cancellationToken);

        return userTraining?.IsCompleted ?? false;
    }
}
