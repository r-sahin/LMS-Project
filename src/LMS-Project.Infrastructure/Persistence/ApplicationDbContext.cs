using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Entities
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserClaim> UserClaims => Set<UserClaim>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Training> Trainings => Set<Training>();
    public DbSet<SubTopic> SubTopics => Set<SubTopic>();
    public DbSet<UserModule> UserModules => Set<UserModule>();
    public DbSet<UserTraining> UserTrainings => Set<UserTraining>();
    public DbSet<UserProgress> UserProgresses => Set<UserProgress>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<ModuleRequest> ModuleRequests => Set<ModuleRequest>();
    public DbSet<TrainingRequest> TrainingRequests => Set<TrainingRequest>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tüm configuration'ları otomatik olarak uygula
        modelBuilder.ApplyConfigurationsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());

        // Global query filter - Soft delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserRole>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserClaim>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Module>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Training>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SubTopic>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserModule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserTraining>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserProgress>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Certificate>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TrainingRequest>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Announcement>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ModuleRequest>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit alanlarını otomatik doldur
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedDate = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
