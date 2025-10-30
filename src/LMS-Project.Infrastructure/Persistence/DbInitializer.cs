using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence;

public class DbInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DbInitializer(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        // Database oluştur (yoksa)
        await _context.Database.EnsureCreatedAsync();

        // Zaten seed yapılmış mı kontrol et
        if (await _context.Users.AnyAsync())
        {
            return; // Database zaten dolu
        }

        // 1. ROLLER OLUŞTUR
        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Description = "Sistem yöneticisi - Tüm yetkilere sahip",
            IsSystemRole = true,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        var moderatorRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Moderator",
            Description = "Modül kayıt taleplerini onaylayan yetkili",
            IsSystemRole = true,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        var studentRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Student",
            Description = "Öğrenci - Eğitimlere erişim",
            IsSystemRole = true,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        await _context.Roles.AddRangeAsync(adminRole, moderatorRole, studentRole);

        // 2. PERMİSSİONS OLUŞTUR
        var permissions = new List<Permission>
        {
            // Module Permissions
            new Permission { Id = Guid.NewGuid(), Name = "Modules.View", Description = "Modülleri görüntüleme", CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "Modules.Create", Description = "Modül oluşturma", CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "Modules.Update", Description = "Modül güncelleme", CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "Modules.Delete", Description = "Modül silme", CreatedBy = "System", CreatedDate = DateTime.UtcNow },

            // Training Permissions
            new Permission { Id = Guid.NewGuid(), Name = "Trainings.Create", Description = "Eğitim oluşturma", CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "Trainings.Update", Description = "Eğitim güncelleme", CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "Trainings.Delete", Description = "Eğitim silme", CreatedBy = "System", CreatedDate = DateTime.UtcNow },

            // ModuleRequest Permissions
            new Permission { Id = Guid.NewGuid(), Name = "ModuleRequests.Approve", Description = "Modül taleplerini onaylama", CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "ModuleRequests.Reject", Description = "Modül taleplerini reddetme", CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "ModuleRequests.View", Description = "Tüm talepleri görüntüleme", CreatedBy = "System", CreatedDate = DateTime.UtcNow },

            // Announcement Permissions
            new Permission { Id = Guid.NewGuid(), Name = "Announcements.Create", Description = "Duyuru oluşturma", CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "Announcements.Update", Description = "Duyuru güncelleme", CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "Announcements.Delete", Description = "Duyuru silme", CreatedBy = "System", CreatedDate = DateTime.UtcNow },
        };

        await _context.Permissions.AddRangeAsync(permissions);

        // 3. ROLE-PERMISSION İLİŞKİLERİ
        var adminPermissions = permissions.Select(p => new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = adminRole.Id,
            PermissionId = p.Id,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        }).ToList();

        var moderatorPermissions = permissions
            .Where(p => p.Name.StartsWith("ModuleRequests."))
            .Select(p => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = moderatorRole.Id,
                PermissionId = p.Id,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            }).ToList();

        await _context.RolePermissions.AddRangeAsync(adminPermissions);
        await _context.RolePermissions.AddRangeAsync(moderatorPermissions);

        // 4. KULLANICILAR OLUŞTUR
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@lms.com",
            PasswordHash = _passwordHasher.HashPassword("Admin123!"),
            FirstName = "Admin",
            LastName = "User",
            IsActive = true,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        var moderatorUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "moderator",
            Email = "moderator@lms.com",
            PasswordHash = _passwordHasher.HashPassword("Moderator123!"),
            FirstName = "Moderator",
            LastName = "User",
            IsActive = true,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        var studentUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "student",
            Email = "student@lms.com",
            PasswordHash = _passwordHasher.HashPassword("Student123!"),
            FirstName = "Student",
            LastName = "User",
            IsActive = true,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        await _context.Users.AddRangeAsync(adminUser, moderatorUser, studentUser);

        // 5. USER-ROLE İLİŞKİLERİ
        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = adminUser.Id, RoleId = adminRole.Id, CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new UserRole { Id = Guid.NewGuid(), UserId = moderatorUser.Id, RoleId = moderatorRole.Id, CreatedBy = "System", CreatedDate = DateTime.UtcNow },
            new UserRole { Id = Guid.NewGuid(), UserId = studentUser.Id, RoleId = studentRole.Id, CreatedBy = "System", CreatedDate = DateTime.UtcNow }
        };

        await _context.UserRoles.AddRangeAsync(userRoles);

        // 6. SAVE
        await _context.SaveChangesAsync();

        Console.WriteLine("✅ Seed Data başarıyla oluşturuldu!");
        Console.WriteLine("");
        Console.WriteLine("📝 Varsayılan Kullanıcılar:");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("👨‍💼 Admin:");
        Console.WriteLine("   Email: admin@lms.com");
        Console.WriteLine("   Password: Admin123!");
        Console.WriteLine("");
        Console.WriteLine("👨‍⚖️ Moderator:");
        Console.WriteLine("   Email: moderator@lms.com");
        Console.WriteLine("   Password: Moderator123!");
        Console.WriteLine("");
        Console.WriteLine("👨‍🎓 Student:");
        Console.WriteLine("   Email: student@lms.com");
        Console.WriteLine("   Password: Student123!");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
}
