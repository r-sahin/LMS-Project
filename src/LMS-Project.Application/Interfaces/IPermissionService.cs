namespace LMS_Project.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(
        Guid userId,
        string permissionName,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> HasRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
