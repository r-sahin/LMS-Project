using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Interfaces;

namespace LMS_Project.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        string permissionName,
        CancellationToken cancellationToken = default)
    {
        // Kullanıcının tüm izinlerini getir
        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);

        // İzin listesinde aranılan izin var mı kontrol et
        return permissions.Contains(permissionName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Kullanıcının sahip olduğu tüm izinleri rol bazlı olarak getir
        var permissions = await _unitOfWork.Permissions.GetPermissionsByUserIdAsync(userId, cancellationToken);

        return permissions
            .Select(p => p.Name)
            .Distinct()
            .ToList();
    }

    public async Task<bool> HasRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var roles = await GetUserRolesAsync(userId, cancellationToken);
        return roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var roles = await _unitOfWork.Roles.GetRolesByUserIdAsync(userId, cancellationToken);

        return roles
            .Select(r => r.Name)
            .ToList();
    }
}
