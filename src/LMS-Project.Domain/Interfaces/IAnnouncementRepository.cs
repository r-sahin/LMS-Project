using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface IAnnouncementRepository : IRepository<Announcement>
{
    Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Announcement>> GetAnnouncementsByRoleAsync(string? role, CancellationToken cancellationToken = default);
    Task<IEnumerable<Announcement>> GetPublishedAnnouncementsAsync(CancellationToken cancellationToken = default);
}
