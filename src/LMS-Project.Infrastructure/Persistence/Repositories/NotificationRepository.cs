using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context) { }
    public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => 
        await _dbSet.Where(n => !n.IsDeleted && n.UserId == userId).OrderByDescending(n => n.CreatedDate).ToListAsync(cancellationToken);
    public async Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => 
        await _dbSet.Where(n => !n.IsDeleted && n.UserId == userId && !n.IsRead).OrderByDescending(n => n.CreatedDate).ToListAsync(cancellationToken);
    public async Task<int> GetUnreadNotificationsCountAsync(Guid userId, CancellationToken cancellationToken = default) => 
        await _dbSet.Where(n => !n.IsDeleted && n.UserId == userId && !n.IsRead).CountAsync(cancellationToken);
}
