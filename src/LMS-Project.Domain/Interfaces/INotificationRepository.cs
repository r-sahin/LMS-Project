using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadNotificationsCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
