using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class TrainingRequestRepository : Repository<TrainingRequest>, ITrainingRequestRepository
{
    public TrainingRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TrainingRequest>> GetRequestsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.TrainingRequests
            .Where(tr => tr.UserId == userId)
            .Include(tr => tr.Training)
            .Include(tr => tr.User)
            .Include(tr => tr.Reviewer)
            .OrderByDescending(tr => tr.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TrainingRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TrainingRequests
            .Where(tr => tr.Status == "Pending")
            .Include(tr => tr.Training)
            .Include(tr => tr.User)
            .OrderBy(tr => tr.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPendingRequestAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default)
    {
        return await _context.TrainingRequests
            .AnyAsync(tr => tr.UserId == userId && tr.TrainingId == trainingId && tr.Status == "Pending", cancellationToken);
    }

    public async Task<TrainingRequest?> GetUserRequestForTrainingAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default)
    {
        return await _context.TrainingRequests
            .FirstOrDefaultAsync(tr => tr.UserId == userId && tr.TrainingId == trainingId, cancellationToken);
    }
}
