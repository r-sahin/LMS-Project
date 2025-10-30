using LMS_Project.Application.DTOs;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.ModuleRequests.Queries;

public record GetPendingModuleRequestsQuery : IRequest<Result<IEnumerable<ModuleRequestDto>>>;

public class GetPendingModuleRequestsQueryHandler : IRequestHandler<GetPendingModuleRequestsQuery, Result<IEnumerable<ModuleRequestDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingModuleRequestsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<ModuleRequestDto>>> Handle(GetPendingModuleRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _unitOfWork.ModuleRequests.GetPendingRequestsAsync(cancellationToken);

        var dtos = new List<ModuleRequestDto>();

        foreach (var req in requests)
        {
            var module = await _unitOfWork.Modules.GetByIdAsync(req.ModuleId, cancellationToken);
            var user = await _unitOfWork.Users.GetByIdAsync(req.UserId, cancellationToken);

            dtos.Add(new ModuleRequestDto
            {
                Id = req.Id,
                UserId = req.UserId,
                UserName = user?.FullName ?? "Unknown",
                UserEmail = user?.Email ?? "",
                ModuleId = req.ModuleId,
                ModuleName = module?.Name ?? "Unknown",
                RequestReason = req.RequestReason,
                Status = req.Status,
                ReviewedBy = req.ReviewedBy,
                ReviewerName = null,
                ReviewedDate = req.ReviewedDate,
                ReviewNote = req.ReviewNote,
                CreatedDate = req.CreatedDate
            });
        }

        return Result<IEnumerable<ModuleRequestDto>>.Success(dtos.OrderBy(x => x.CreatedDate));
    }
}
