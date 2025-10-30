using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class UserTrainingRepository : Repository<UserTraining>, IUserTrainingRepository
{
    public UserTrainingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserTraining?> GetByUserAndTrainingAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(ut => ut.Training).Where(ut => !ut.IsDeleted)
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TrainingId == trainingId, cancellationToken);
    }

    public async Task<IEnumerable<UserTraining>> GetTrainingsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(ut => ut.Training).Where(ut => !ut.IsDeleted && ut.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserTraining>> GetCompletedTrainingsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(ut => ut.Training).Where(ut => !ut.IsDeleted && ut.UserId == userId && ut.IsCompleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserTraining>> GetTrainingsByUserAndModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(ut => ut.Training).Where(ut => !ut.IsDeleted && ut.UserId == userId && ut.Training.ModuleId == moduleId).ToListAsync(cancellationToken);
    }
}
