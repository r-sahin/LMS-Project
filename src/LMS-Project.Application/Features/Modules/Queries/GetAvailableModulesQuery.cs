using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Modules.Queries;

/// <summary>
/// Kullanıcının kayıtlı olmadığı, talep edebileceği YAYINLANMIŞ modülleri getirir
/// </summary>
public record GetAvailableModulesQuery : IRequest<Result<List<AvailableModuleDto>>>;

public class GetAvailableModulesQueryHandler : IRequestHandler<GetAvailableModulesQuery, Result<List<AvailableModuleDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAvailableModulesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<AvailableModuleDto>>> Handle(
        GetAvailableModulesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // ⭐ SADECE YAYINLANMIŞ VE AKTİF modülleri getir
        var allModules = await _unitOfWork.Modules.GetActiveModulesAsync(cancellationToken);

        // Kullanıcının kayıtlı olduğu modülleri getir
        var enrolledModuleIds = (await _unitOfWork.UserModules.GetModulesByUserIdAsync(userId, cancellationToken))
            .Select(um => um.ModuleId)
            .ToHashSet();

        var availableModules = new List<AvailableModuleDto>();

        // ⚠️ ÖNEMLİ: Sadece yayınlanmış, kayıtlı olmadığımız modüller
        foreach (var module in allModules.Where(m => m.IsPublished && !enrolledModuleIds.Contains(m.Id)))
        {
            // Bekleyen talep var mı kontrol et
            var hasPendingRequest = await _unitOfWork.ModuleRequests
                .HasPendingRequestAsync(userId, module.Id, cancellationToken);

            // Son talebin durumunu getir
            var lastRequest = await _unitOfWork.ModuleRequests
                .GetUserRequestForModuleAsync(userId, module.Id, cancellationToken);

            availableModules.Add(new AvailableModuleDto
            {
                Id = module.Id,
                Name = module.Name,
                Description = module.Description ?? string.Empty,
                ImagePath = module.ImagePath,
                EstimatedDurationMinutes = module.EstimatedDurationMinutes,
                HasPendingRequest = hasPendingRequest,
                LastRequestStatus = lastRequest?.Status,
                LastRequestDate = lastRequest?.CreatedDate
            });
        }

        return Result<List<AvailableModuleDto>>.Success(
            availableModules.OrderBy(m => m.Name).ToList(),
            "Kayıt olabileceğiniz modüller getirildi.");
    }
}
