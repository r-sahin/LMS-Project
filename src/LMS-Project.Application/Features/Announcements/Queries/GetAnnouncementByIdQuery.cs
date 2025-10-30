using LMS_Project.Application.DTOs;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Announcements.Queries;

public record GetAnnouncementByIdQuery(Guid Id) : IRequest<Result<AnnouncementDto>>;

public class GetAnnouncementByIdQueryHandler : IRequestHandler<GetAnnouncementByIdQuery, Result<AnnouncementDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAnnouncementByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AnnouncementDto>> Handle(GetAnnouncementByIdQuery request, CancellationToken cancellationToken)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdAsync(request.Id, cancellationToken);
        if (announcement == null)
            return Result<AnnouncementDto>.Failure("Duyuru bulunamadı.");

        var dto = new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            PublishDate = announcement.PublishDate,
            ExpiryDate = announcement.ExpiryDate,
            IsActive = announcement.IsActive,
            Priority = announcement.Priority,
            TargetRole = announcement.TargetRole,
            ImagePath = announcement.ImagePath,
            CreatedDate = announcement.CreatedDate
        };

        return Result<AnnouncementDto>.Success(dto);
    }
}
