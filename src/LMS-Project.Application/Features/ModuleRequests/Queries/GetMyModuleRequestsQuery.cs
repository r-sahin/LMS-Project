using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.ModuleRequests.Queries;

public record GetMyModuleRequestsQuery : IRequest<Result<IEnumerable<ModuleRequestDto>>>;

public class GetMyModuleRequestsQueryHandler : IRequestHandler<GetMyModuleRequestsQuery, Result<IEnumerable<ModuleRequestDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyModuleRequestsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<ModuleRequestDto>>> Handle(GetMyModuleRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var requests = await _unitOfWork.ModuleRequests.GetRequestsByUserIdAsync(userId, cancellationToken);

        // ⚠️ Kullanıcının hiç talebi yoksa hata dön
        if (!requests.Any())
        {
            return Result<IEnumerable<ModuleRequestDto>>.Failure("Size ait hiçbir modül kaydı talebi bulunamadı.");
        }

        var dtos = new List<ModuleRequestDto>();

        foreach (var req in requests)
        {
            var module = await _unitOfWork.Modules.GetByIdAsync(req.ModuleId, cancellationToken);
            var user = await _unitOfWork.Users.GetByIdAsync(req.UserId, cancellationToken);

            User? reviewer = null;
            if (req.ReviewedBy.HasValue)
            {
                reviewer = await _unitOfWork.Users.GetByIdAsync(req.ReviewedBy.Value, cancellationToken);
            }

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
                ReviewerName = reviewer?.FullName,
                ReviewedDate = req.ReviewedDate,
                ReviewNote = req.ReviewNote,
                CreatedDate = req.CreatedDate
            });
        }

        return Result<IEnumerable<ModuleRequestDto>>.Success(dtos.OrderByDescending(x => x.CreatedDate));
    }
}
