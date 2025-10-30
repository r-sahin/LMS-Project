using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Announcements
            .Where(a => !a.IsDeleted
                && a.IsActive
                && a.PublishDate <= now
                && (!a.ExpiryDate.HasValue || a.ExpiryDate.Value > now))
            .OrderByDescending(a => a.Priority == "Urgent")
            .ThenByDescending(a => a.Priority == "High")
            .ThenByDescending(a => a.PublishDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Announcement>> GetAnnouncementsByRoleAsync(string? role, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Announcements
            .Where(a => !a.IsDeleted
                && a.IsActive
                && a.PublishDate <= now
                && (!a.ExpiryDate.HasValue || a.ExpiryDate.Value > now)
                && (a.TargetRole == null || a.TargetRole == role))
            .OrderByDescending(a => a.Priority == "Urgent")
            .ThenByDescending(a => a.Priority == "High")
            .ThenByDescending(a => a.PublishDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Announcement>> GetPublishedAnnouncementsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Announcements
            .Where(a => !a.IsDeleted && a.PublishDate <= now)
            .OrderByDescending(a => a.PublishDate)
            .ToListAsync(cancellationToken);
    }
}
