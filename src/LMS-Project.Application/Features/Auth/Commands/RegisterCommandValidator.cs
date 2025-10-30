using FluentValidation;

namespace LMS_Project.Application.Features.Auth.Commands;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("Kullanıcı adı boş olamaz.")
            .MinimumLength(3)
            .WithMessage("Kullanıcı adı en az 3 karakter olmalıdır.")
            .MaximumLength(50)
            .WithMessage("Kullanıcı adı en fazla 50 karakter olabilir.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email boş olamaz.")
            .EmailAddress()
            .WithMessage("Geçerli bir email adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Şifre boş olamaz.")
            .MinimumLength(6)
            .WithMessage("Şifre en az 6 karakter olmalıdır.")
            .Matches(@"[A-Z]")
            .WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches(@"[a-z]")
            .WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches(@"[0-9]")
            .WithMessage("Şifre en az bir rakam içermelidir.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Şifreler eşleşmiyor.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Ad boş olamaz.")
            .MaximumLength(50)
            .WithMessage("Ad en fazla 50 karakter olabilir.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Soyad boş olamaz.")
            .MaximumLength(50)
            .WithMessage("Soyad en fazla 50 karakter olabilir.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+90|0)?[0-9]{10}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Geçerli bir telefon numarası giriniz.");
    }
}
