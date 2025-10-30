using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class UserProgressRepository : Repository<UserProgress>, IUserProgressRepository
{
    public UserProgressRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserProgress?> GetProgressByUserAndSubTopicAsync(
        Guid userId,
        Guid subTopicId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(up => up.SubTopic)
            .Where(up => !up.IsDeleted)
            .FirstOrDefaultAsync(
                up => up.UserId == userId && up.SubTopicId == subTopicId,
                cancellationToken);
    }

    public async Task<IEnumerable<UserProgress>> GetProgressesByUserAndTrainingAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(up => up.SubTopic)
            .Where(up => !up.IsDeleted)
            .Where(up => up.UserId == userId && up.SubTopic.TrainingId == trainingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserProgress>> GetProgressesByUserAndModuleAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(up => up.SubTopic)
                .ThenInclude(st => st.Training)
            .Where(up => !up.IsDeleted)
            .Where(up => up.UserId == userId && up.SubTopic.Training.ModuleId == moduleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCompletedSubTopicsCountByTrainingAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(up => !up.IsDeleted)
            .Where(up => up.UserId == userId &&
                         up.SubTopic.TrainingId == trainingId &&
                         up.IsCompleted)
            .CountAsync(cancellationToken);
    }

    public async Task<bool> HasUserCompletedPreviousSubTopicAsync(
        Guid userId,
        Guid trainingId,
        int orderIndex,
        CancellationToken cancellationToken = default)
    {
        if (orderIndex == 0)
        {
            return true; // İlk alt başlık için önceki kontrol yok
        }

        var previousSubTopic = await _context.Set<SubTopic>()
            .Where(st => !st.IsDeleted)
            .Where(st => st.TrainingId == trainingId && st.OrderIndex == orderIndex - 1)
            .FirstOrDefaultAsync(cancellationToken);

        if (previousSubTopic == null)
        {
            return false;
        }

        var previousProgress = await GetProgressByUserAndSubTopicAsync(
            userId,
            previousSubTopic.Id,
            cancellationToken);

        return previousProgress?.IsCompleted ?? false;
    }
}
