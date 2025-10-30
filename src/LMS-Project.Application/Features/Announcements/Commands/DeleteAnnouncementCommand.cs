using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Announcements.Commands;

public record DeleteAnnouncementCommand(Guid Id) : IRequest<Result>;

public class DeleteAnnouncementCommandHandler : IRequestHandler<DeleteAnnouncementCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public DeleteAnnouncementCommandHandler(IUnitOfWork unitOfWork, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    public async Task<Result> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdAsync(request.Id, cancellationToken);
        if (announcement == null)
            return Result.Failure("Duyuru bulunamadı.");

        // Delete image if exists
        if (!string.IsNullOrEmpty(announcement.ImagePath))
        {
            await _fileService.DeleteAnnouncementImageAsync(announcement.ImagePath, cancellationToken);
        }

        _unitOfWork.Announcements.SoftDelete(announcement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Duyuru başarıyla silindi.");
    }
}
