using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace LMS_Project.Application.Features.Modules.Commands;

public record DeleteModuleCommand(Guid Id) : IRequest<Result>;

public class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Modül ID gereklidir.");
    }
}

public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public DeleteModuleCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _unitOfWork.Modules.GetByIdAsync(request.Id, cancellationToken);
        if (module == null)
        {
            return Result.Failure("Modül bulunamadı.");
        }

        // Soft delete
        module.IsDeleted = true;
        module.DeletedDate = DateTime.UtcNow;
        module.DeletedBy = _currentUserService.UserId.ToString();

        _unitOfWork.Modules.Update(module);

        // Fiziksel dosyaları sil
        await _fileService.DeleteModuleFilesAsync(request.Id, cancellationToken);

        // Resmi sil
        if (!string.IsNullOrEmpty(module.ImagePath))
        {
            await _fileService.DeleteFileAsync(module.ImagePath, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Modül başarıyla silindi.");
    }
}
