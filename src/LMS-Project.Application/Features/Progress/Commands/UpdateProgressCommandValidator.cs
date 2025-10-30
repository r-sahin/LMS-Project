using FluentValidation;

namespace LMS_Project.Application.Features.Progress.Commands;

public class UpdateProgressCommandValidator : AbstractValidator<UpdateProgressCommand>
{
    public UpdateProgressCommandValidator()
    {
        RuleFor(x => x.SubTopicId)
            .NotEmpty()
            .WithMessage("Alt başlık Id boş olamaz.");

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0)
            .WithMessage("Süre 0'dan büyük olmalıdır.")
            .LessThan(86400)
            .WithMessage("Süre 24 saatten fazla olamaz.");
    }
}
