namespace LMS_Project.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IModuleRepository Modules { get; }
    ITrainingRepository Trainings { get; }
    ISubTopicRepository SubTopics { get; }
    IUserRepository Users { get; }
    IUserModuleRepository UserModules { get; }
    IUserTrainingRepository UserTrainings { get; }
    IUserProgressRepository UserProgresses { get; }
    ICertificateRepository Certificates { get; }
    IRoleRepository Roles { get; }
    IPermissionRepository Permissions { get; }
    IUserRoleRepository UserRoles { get; }
    IRolePermissionRepository RolePermissions { get; }
    INotificationRepository Notifications { get; }
    IAnnouncementRepository Announcements { get; }
    IModuleRequestRepository ModuleRequests { get; }
    ITrainingRequestRepository TrainingRequests { get; }
    IAuditLogRepository AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
