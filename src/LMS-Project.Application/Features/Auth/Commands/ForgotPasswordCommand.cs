using FluentValidation;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;
using System.Security.Cryptography;

namespace LMS_Project.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email gereklidir.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

        // Güvenlik için: Email bulunamasa bile başarılı dönüyoruz
        // (Kötü niyetli kişilerin sistemde hangi emaillerin kayıtlı olduğunu öğrenmesini engellemek için)
        if (user == null)
            return Result.Success("Eğer email adresiniz sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderilecektir.");

        if (!user.IsActive)
            return Result.Success("Eğer email adresiniz sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderilecektir.");

        // Generate reset token
        var resetToken = GenerateResetToken();
        var resetTokenExpiry = DateTime.UtcNow.AddHours(1); // 1 saat geçerli

        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = resetTokenExpiry;
        user.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email
        await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, cancellationToken);

        return Result.Success("Eğer email adresiniz sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderilecektir.");
    }

    private string GenerateResetToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
