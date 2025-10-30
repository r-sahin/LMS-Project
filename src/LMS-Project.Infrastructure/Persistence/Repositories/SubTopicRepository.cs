using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class SubTopicRepository : Repository<SubTopic>, ISubTopicRepository
{
    public SubTopicRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SubTopic>> GetSubTopicsByTrainingIdAsync(
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(st => !st.IsDeleted)
            .Where(st => st.TrainingId == trainingId)
            .OrderBy(st => st.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubTopic?> GetNextSubTopicAsync(
        Guid trainingId,
        int currentOrderIndex,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(st => !st.IsDeleted && st.IsActive)
            .Where(st => st.TrainingId == trainingId && st.OrderIndex > currentOrderIndex)
            .OrderBy(st => st.OrderIndex)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SubTopic?> GetFirstSubTopicOfTrainingAsync(
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(st => !st.IsDeleted && st.IsActive)
            .Where(st => st.TrainingId == trainingId)
            .OrderBy(st => st.OrderIndex)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsSubTopicCompletedByUserAsync(
        Guid userId,
        Guid subTopicId,
        CancellationToken cancellationToken = default)
    {
        var userProgress = await _context.Set<UserProgress>()
            .Where(up => !up.IsDeleted)
            .FirstOrDefaultAsync(
                up => up.UserId == userId && up.SubTopicId == subTopicId,
                cancellationToken);

        return userProgress?.IsCompleted ?? false;
    }
}
