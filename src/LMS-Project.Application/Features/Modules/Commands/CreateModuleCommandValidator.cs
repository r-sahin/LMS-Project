using FluentValidation;

namespace LMS_Project.Application.Features.Modules.Commands;

public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Modül adı boş olamaz.")
            .MaximumLength(200).WithMessage("Modül adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Tahmini süre 0'dan büyük olmalıdır.");
    }
}
