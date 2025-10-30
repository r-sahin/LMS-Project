using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;

namespace LMS_Project.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPermissionService _permissionService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _permissionService = permissionService;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // 1. Kullanıcıyı email veya kullanıcı adı ile bul
        User? user = null;

        if (request.UserNameOrEmail.Contains("@"))
        {
            user = await _unitOfWork.Users.GetByEmailAsync(request.UserNameOrEmail, cancellationToken);
        }
        else
        {
            user = await _unitOfWork.Users.GetByUserNameAsync(request.UserNameOrEmail, cancellationToken);
        }

        if (user == null)
        {
            return Result<LoginResponseDto>.Failure("Kullanıcı adı veya şifre hatalı.");
        }

        // 2. Kullanıcı aktif mi kontrol et
        if (!user.IsActive)
        {
            return Result<LoginResponseDto>.Failure("Hesabınız pasif durumda. Lütfen yönetici ile iletişime geçin.");
        }

        // 3. Şifre kontrolü
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<LoginResponseDto>.Failure("Kullanıcı adı veya şifre hatalı.");
        }

        // 4. Kullanıcının rollerini ve izinlerini getir
        var roles = await _permissionService.GetUserRolesAsync(user.Id, cancellationToken);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id, cancellationToken);

        // 5. JWT token oluştur
        var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // 6. Refresh token'ı kaydet
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // 7 gün geçerli
        user.LastLoginDate = DateTime.UtcNow;
        user.UpdatedDate = DateTime.UtcNow;
        user.UpdatedBy = user.Id.ToString();

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Response oluştur
        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1), // Access token 1 saat geçerli
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                ProfilePicturePath = user.ProfilePicturePath,
                PhoneNumber = user.PhoneNumber,
                Roles = roles,
                Permissions = permissions
            }
        };

        return Result<LoginResponseDto>.Success(response, "Giriş başarılı.");
    }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // 1. Refresh token ile kullanıcıyı bul
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.RefreshToken == request.RefreshToken,
            cancellationToken);

        if (user == null)
        {
            return Result<LoginResponseDto>.Failure("Geçersiz refresh token.");
        }

        // 2. Refresh token'ın süresini kontrol et
        if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return Result<LoginResponseDto>.Failure("Refresh token süresi dolmuş. Lütfen tekrar giriş yapın.");
        }

        // 3. Kullanıcı aktif mi kontrol et
        if (!user.IsActive)
        {
            return Result<LoginResponseDto>.Failure("Hesabınız pasif durumda.");
        }

        // 4. Kullanıcının rollerini ve izinlerini getir
        var roles = await _permissionService.GetUserRolesAsync(user.Id, cancellationToken);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id, cancellationToken);

        // 5. Yeni token'lar oluştur
        var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // 6. Yeni refresh token'ı kaydet
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.UpdatedDate = DateTime.UtcNow;
        user.UpdatedBy = user.Id.ToString();

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Response oluştur
        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                ProfilePicturePath = user.ProfilePicturePath,
                PhoneNumber = user.PhoneNumber,
                Roles = roles,
                Permissions = permissions
            }
        };

        return Result<LoginResponseDto>.Success(response, "Token yenilendi.");
    }

    public async Task<Result> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // 1. Email kontrolü
        var emailExists = await _unitOfWork.Users.IsEmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
        {
            return Result.Failure("Bu email adresi zaten kullanılıyor.");
        }

        // 2. Kullanıcı adı kontrolü
        var userNameExists = await _unitOfWork.Users.IsUserNameExistsAsync(request.UserName, cancellationToken);
        if (userNameExists)
        {
            return Result.Failure("Bu kullanıcı adı zaten kullanılıyor.");
        }

        // 3. Şifrelerin eşleştiğini kontrol et (validator'da da kontrol ediliyor ama yine de)
        if (request.Password != request.ConfirmPassword)
        {
            return Result.Failure("Şifreler eşleşmiyor.");
        }

        // 4. Kullanıcı oluştur
        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            CreatedBy = "System"
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Varsayılan "User" rolünü ata
        var userRole = await _unitOfWork.Roles.GetByNameAsync("User", cancellationToken);
        if (userRole != null)
        {
            var userRoleAssignment = new UserRole
            {
                UserId = user.Id,
                RoleId = userRole.Id,
                CreatedBy = "System"
            };

            await _unitOfWork.UserRoles.AddAsync(userRoleAssignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success("Kayıt başarılı. Giriş yapabilirsiniz.");
    }

    public async Task<Result> LogoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure("Kullanıcı bulunamadı.");
        }

        // Refresh token'ı temizle
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedDate = DateTime.UtcNow;
        user.UpdatedBy = userId.ToString();

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Çıkış başarılı.");
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure("Kullanıcı bulunamadı.");
        }

        // Mevcut şifreyi kontrol et
        if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
        {
            return Result.Failure("Mevcut şifre hatalı.");
        }

        // Yeni şifreyi hashle ve kaydet
        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.UpdatedDate = DateTime.UtcNow;
        user.UpdatedBy = userId.ToString();

        // Güvenlik için refresh token'ı da temizle
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Şifre başarıyla değiştirildi. Lütfen tekrar giriş yapın.");
    }
}
