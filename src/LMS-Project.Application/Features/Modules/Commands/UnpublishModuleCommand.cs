using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace LMS_Project.Application.Features.Modules.Commands;

public record UnpublishModuleCommand(Guid ModuleId) : IRequest<Result>;

public class UnpublishModuleCommandValidator : AbstractValidator<UnpublishModuleCommand>
{
    public UnpublishModuleCommandValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty().WithMessage("Modül ID gereklidir.");
    }
}

public class UnpublishModuleCommandHandler : IRequestHandler<UnpublishModuleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UnpublishModuleCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UnpublishModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module == null)
        {
            return Result.Failure("Modül bulunamadı.");
        }

        if (!module.IsPublished)
        {
            return Result.Failure("Modül zaten yayınlanmamış durumda.");
        }

        // Modülü yayından kaldır
        module.IsPublished = false;
        module.PublishedDate = null;
        module.PublishedBy = null;
        module.UpdatedBy = _currentUserService.UserId.ToString();
        module.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Modules.Update(module);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Modül başarıyla yayından kaldırıldı.");
    }
}
