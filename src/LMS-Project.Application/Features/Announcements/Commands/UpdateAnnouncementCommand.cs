using FluentValidation;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS_Project.Application.Features.Announcements.Commands;

public record UpdateAnnouncementCommand(
    Guid Id,
    string Title,
    string Content,
    DateTime PublishDate,
    DateTime? ExpiryDate,
    string Priority,
    string? TargetRole,
    bool IsActive,
    IFormFile? ImageFile) : IRequest<Result>;

public class UpdateAnnouncementCommandValidator : AbstractValidator<UpdateAnnouncementCommand>
{
    public UpdateAnnouncementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Priority)
            .Must(p => p == "Low" || p == "Normal" || p == "High" || p == "Urgent");

        When(x => x.ExpiryDate.HasValue, () =>
        {
            RuleFor(x => x.ExpiryDate)
                .GreaterThan(x => x.PublishDate);
        });

        When(x => x.ImageFile != null, () =>
        {
            RuleFor(x => x.ImageFile!.Length).LessThanOrEqualTo(10 * 1024 * 1024);
            RuleFor(x => x.ImageFile!.ContentType).Must(ct => ct.StartsWith("image/"));
        });
    }
}

public class UpdateAnnouncementCommandHandler : IRequestHandler<UpdateAnnouncementCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public UpdateAnnouncementCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdAsync(request.Id, cancellationToken);
        if (announcement == null)
            return Result.Failure("Duyuru bulunamadı.");

        // Upload new image if provided
        if (request.ImageFile != null)
        {
            // Delete old image
            if (!string.IsNullOrEmpty(announcement.ImagePath))
            {
                await _fileService.DeleteAnnouncementImageAsync(announcement.ImagePath, cancellationToken);
            }

            var uploadResult = await _fileService.UploadAnnouncementImageAsync(
                request.ImageFile,
                cancellationToken);

            if (!uploadResult.IsSuccess)
                return Result.Failure(uploadResult.Errors);

            announcement.ImagePath = uploadResult.Data;
        }

        announcement.Title = request.Title;
        announcement.Content = request.Content;
        announcement.PublishDate = request.PublishDate;
        announcement.ExpiryDate = request.ExpiryDate;
        announcement.Priority = request.Priority;
        announcement.TargetRole = request.TargetRole;
        announcement.IsActive = request.IsActive;
        announcement.UpdatedBy = _currentUserService.UserId.ToString();
        announcement.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Announcements.Update(announcement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Duyuru başarıyla güncellendi.");
    }
}
