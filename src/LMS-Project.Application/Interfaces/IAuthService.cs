using LMS_Project.Application.DTOs;
using LMS_Project.Domain.Common;

namespace LMS_Project.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<LoginResponseDto>> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);
}
