using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS_Project.Application.Features.Modules.Commands;

public record UpdateModuleCommand(
    Guid Id,
    string Name,
    string? Description,
    int EstimatedDurationMinutes,
    bool IsActive,
    IFormFile? ImageFile) : IRequest<Result>;

public class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Modül ID gereklidir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Modül adı gereklidir.")
            .MaximumLength(200).WithMessage("Modül adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Tahmini süre 0'dan büyük olmalıdır.");
    }
}

public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public UpdateModuleCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _unitOfWork.Modules.GetByIdAsync(request.Id, cancellationToken);
        if (module == null)
        {
            return Result.Failure("Modül bulunamadı.");
        }

        // Yeni resim yüklendiyse
        if (request.ImageFile != null)
        {
            // Eski resmi sil
            if (!string.IsNullOrEmpty(module.ImagePath))
            {
                await _fileService.DeleteFileAsync(module.ImagePath, cancellationToken);
            }

            // Yeni resmi yükle
            var uploadResult = await _fileService.UploadFileAsync(
                request.ImageFile,
                "images/modules",
                cancellationToken);

            if (!uploadResult.IsSuccess)
            {
                return Result.Failure(uploadResult.Message);
            }

            module.ImagePath = uploadResult.Data;
        }

        module.Name = request.Name;
        module.Description = request.Description;
        module.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        module.IsActive = request.IsActive;
        module.UpdatedBy = _currentUserService.UserId.ToString();
        module.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Modules.Update(module);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Modül başarıyla güncellendi.");
    }
}
