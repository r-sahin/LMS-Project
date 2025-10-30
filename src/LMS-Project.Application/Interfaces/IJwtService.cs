using LMS_Project.Domain.Entities;
using System.Security.Claims;

namespace LMS_Project.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}
