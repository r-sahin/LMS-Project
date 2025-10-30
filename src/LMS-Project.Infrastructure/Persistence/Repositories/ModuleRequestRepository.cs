using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class ModuleRequestRepository : Repository<ModuleRequest>, IModuleRequestRepository
{
    public ModuleRequestRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<ModuleRequest>> GetRequestsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ModuleRequests
            .Where(mr => !mr.IsDeleted && mr.UserId == userId)
            .OrderByDescending(mr => mr.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ModuleRequest>> GetRequestsByModuleIdAsync(Guid moduleId, CancellationToken cancellationToken = default)
    {
        return await _context.ModuleRequests
            .Where(mr => !mr.IsDeleted && mr.ModuleId == moduleId)
            .OrderByDescending(mr => mr.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ModuleRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ModuleRequests
            .Where(mr => !mr.IsDeleted && mr.Status == "Pending")
            .OrderBy(mr => mr.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<ModuleRequest?> GetUserRequestForModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default)
    {
        return await _context.ModuleRequests
            .Where(mr => !mr.IsDeleted && mr.UserId == userId && mr.ModuleId == moduleId)
            .OrderByDescending(mr => mr.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasPendingRequestAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default)
    {
        return await _context.ModuleRequests
            .AnyAsync(mr => !mr.IsDeleted
                && mr.UserId == userId
                && mr.ModuleId == moduleId
                && mr.Status == "Pending",
                cancellationToken);
    }
}
