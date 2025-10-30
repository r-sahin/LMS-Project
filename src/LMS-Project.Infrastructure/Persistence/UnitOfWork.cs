using LMS_Project.Domain.Interfaces;
using LMS_Project.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace LMS_Project.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        // Repository'leri başlat
        Modules = new ModuleRepository(_context);
        Trainings = new TrainingRepository(_context);
        SubTopics = new SubTopicRepository(_context);
        Users = new UserRepository(_context);
        UserModules = new UserModuleRepository(_context);
        UserTrainings = new UserTrainingRepository(_context);
        UserProgresses = new UserProgressRepository(_context);
        Certificates = new CertificateRepository(_context);
        Roles = new RoleRepository(_context);
        Permissions = new PermissionRepository(_context);
        UserRoles = new UserRoleRepository(_context);
        RolePermissions = new RolePermissionRepository(_context);
        Notifications = new NotificationRepository(_context);
        Announcements = new AnnouncementRepository(_context);
        ModuleRequests = new ModuleRequestRepository(_context);
        TrainingRequests = new TrainingRequestRepository(_context);
        AuditLogs = new AuditLogRepository(_context);
    }

    public IModuleRepository Modules { get; }
    public ITrainingRepository Trainings { get; }
    public ISubTopicRepository SubTopics { get; }
    public IUserRepository Users { get; }
    public IUserModuleRepository UserModules { get; }
    public IUserTrainingRepository UserTrainings { get; }
    public IUserProgressRepository UserProgresses { get; }
    public ICertificateRepository Certificates { get; }
    public IRoleRepository Roles { get; }
    public IPermissionRepository Permissions { get; }
    public IUserRoleRepository UserRoles { get; }
    public IRolePermissionRepository RolePermissions { get; }
    public INotificationRepository Notifications { get; }
    public IAnnouncementRepository Announcements { get; }
    public IModuleRequestRepository ModuleRequests { get; }
    public ITrainingRequestRepository TrainingRequests { get; }
    public IAuditLogRepository AuditLogs { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
