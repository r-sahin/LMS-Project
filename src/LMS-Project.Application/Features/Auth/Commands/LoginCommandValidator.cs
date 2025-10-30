using FluentValidation;

namespace LMS_Project.Application.Features.Auth.Commands;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserNameOrEmail)
            .NotEmpty()
            .WithMessage("Kullanıcı adı veya email boş olamaz.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Şifre boş olamaz.");
    }
}
