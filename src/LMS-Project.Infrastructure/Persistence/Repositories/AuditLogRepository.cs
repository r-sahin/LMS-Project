using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context) { }
    public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => 
        await _dbSet.Where(al => !al.IsDeleted && al.UserId == userId).OrderByDescending(al => al.CreatedDate).ToListAsync(cancellationToken);
    public async Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default) => 
        await _dbSet.Where(al => !al.IsDeleted && al.EntityType == entityType && al.EntityId == entityId).OrderByDescending(al => al.CreatedDate).ToListAsync(cancellationToken);
    public async Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default) => 
        await _dbSet.Where(al => !al.IsDeleted && al.CreatedDate >= startDate && al.CreatedDate <= endDate).OrderByDescending(al => al.CreatedDate).ToListAsync(cancellationToken);
}
