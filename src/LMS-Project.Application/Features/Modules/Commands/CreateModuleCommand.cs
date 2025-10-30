using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS_Project.Application.Features.Modules.Commands;

public record CreateModuleCommand(
    string Name,
    string? Description,
    int EstimatedDurationMinutes,
    IFormFile? ImageFile) : IRequest<Result<Guid>>;



public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public CreateModuleCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result<Guid>> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        // En yüksek OrderIndex'i bul
        var modules = await _unitOfWork.Modules.GetAllAsync(cancellationToken);
        var maxOrderIndex = modules.Any() ? modules.Max(m => m.OrderIndex) : -1;

        string? imagePath = null;
        if (request.ImageFile != null)
        {
            var uploadResult = await _fileService.UploadFileAsync(
                request.ImageFile,
                "images/modules",
                cancellationToken);

            if (!uploadResult.IsSuccess)
            {
                return Result<Guid>.Failure(uploadResult.Message);
            }

            imagePath = uploadResult.Data;
        }

        var module = new Module
        {
            Name = request.Name,
            Description = request.Description,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            ImagePath = imagePath,
            OrderIndex = maxOrderIndex + 1,
            IsActive = true,
            CreatedBy = _currentUserService.UserId.ToString()
        };

        await _unitOfWork.Modules.AddAsync(module, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(module.Id, "Modül başarıyla oluşturuldu.");
    }
}
