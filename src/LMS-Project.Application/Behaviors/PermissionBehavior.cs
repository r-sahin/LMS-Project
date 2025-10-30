using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using MediatR;
using System.Reflection;

namespace LMS_Project.Application.Behaviors;

public class PermissionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;

    public PermissionBehavior(
        ICurrentUserService currentUserService,
        IPermissionService permissionService)
    {
        _currentUserService = currentUserService;
        _permissionService = permissionService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType()
            .GetCustomAttributes<AuthorizeAttribute>()
            .ToList();

        if (!authorizeAttributes.Any())
        {
            return await next();
        }

        var userId = _currentUserService.UserId;
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Kullanıcı kimlik bilgisi bulunamadı.");
        }

        foreach (var authorizeAttribute in authorizeAttributes)
        {
            var hasPermission = await _permissionService.HasPermissionAsync(
                userId,
                authorizeAttribute.Permission,
                cancellationToken);

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException(
                    $"Bu işlem için '{authorizeAttribute.Permission}' yetkisine sahip değilsiniz.");
            }
        }

        return await next();
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizeAttribute : Attribute
{
    public string Permission { get; }

    public AuthorizeAttribute(string permission)
    {
        Permission = permission;
    }
}
