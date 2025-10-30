using FluentValidation;
using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS_Project.Application.Features.Announcements.Commands;

public record CreateAnnouncementCommand(
    string Title,
    string Content,
    DateTime PublishDate,
    DateTime? ExpiryDate,
    string Priority,
    string? TargetRole,
    IFormFile? ImageFile) : IRequest<Result<Guid>>;

public class CreateAnnouncementCommandValidator : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık gereklidir.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik gereklidir.");

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => p == "Low" || p == "Normal" || p == "High" || p == "Urgent")
            .WithMessage("Öncelik Low, Normal, High veya Urgent olmalıdır.");

        RuleFor(x => x.PublishDate)
            .NotEmpty().WithMessage("Yayın tarihi gereklidir.");

        When(x => x.ExpiryDate.HasValue, () =>
        {
            RuleFor(x => x.ExpiryDate)
                .GreaterThan(x => x.PublishDate)
                .WithMessage("Son geçerlilik tarihi yayın tarihinden sonra olmalıdır.");
        });

        When(x => x.ImageFile != null, () =>
        {
            RuleFor(x => x.ImageFile!.Length)
                .LessThanOrEqualTo(10 * 1024 * 1024)
                .WithMessage("Resim dosyası en fazla 10MB olabilir.");

            RuleFor(x => x.ImageFile!.ContentType)
                .Must(ct => ct.StartsWith("image/"))
                .WithMessage("Sadece resim dosyaları yüklenebilir.");
        });
    }
}

public class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public CreateAnnouncementCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result<Guid>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        string? imagePath = null;

        // Upload image if provided
        if (request.ImageFile != null)
        {
            var uploadResult = await _fileService.UploadAnnouncementImageAsync(
                request.ImageFile,
                cancellationToken);

            if (!uploadResult.IsSuccess)
                return Result<Guid>.Failure(uploadResult.Errors);

            imagePath = uploadResult.Data;
        }

        var announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            PublishDate = request.PublishDate,
            ExpiryDate = request.ExpiryDate,
            Priority = request.Priority,
            TargetRole = request.TargetRole,
            ImagePath = imagePath,
            IsActive = true,
            CreatedBy = _currentUserService.UserId.ToString(),
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Announcements.AddAsync(announcement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(announcement.Id, "Duyuru başarıyla oluşturuldu.");
    }
}
