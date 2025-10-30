using LMS_Project.Application.DTOs;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Announcements.Queries;

public record GetAnnouncementsQuery(bool OnlyActive = true) : IRequest<Result<IEnumerable<AnnouncementDto>>>;

public class GetAnnouncementsQueryHandler : IRequestHandler<GetAnnouncementsQuery, Result<IEnumerable<AnnouncementDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAnnouncementsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<AnnouncementDto>>> Handle(GetAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var announcements = request.OnlyActive
            ? await _unitOfWork.Announcements.GetActiveAnnouncementsAsync(cancellationToken)
            : await _unitOfWork.Announcements.GetAllAsync(cancellationToken);

        var dtos = announcements.Select(a => new AnnouncementDto
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            PublishDate = a.PublishDate,
            ExpiryDate = a.ExpiryDate,
            IsActive = a.IsActive,
            Priority = a.Priority,
            TargetRole = a.TargetRole,
            ImagePath = a.ImagePath,
            CreatedDate = a.CreatedDate
        }).ToList();

        return Result<IEnumerable<AnnouncementDto>>.Success(dtos);
    }
}
