using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Modules.Queries;

public record GetAllModulesQuery : IRequest<Result<List<ModuleListDto>>>;

public class GetAllModulesQueryHandler : IRequestHandler<GetAllModulesQuery, Result<List<ModuleListDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAllModulesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ModuleListDto>>> Handle(
        GetAllModulesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // ⭐ SADECE KULLANICININ KAYITLI OLDUĞU MODÜLLER
        var userModules = await _unitOfWork.UserModules.GetModulesByUserIdAsync(userId, cancellationToken);

        // ⚠️ Kullanıcıya atanmış hiç modül yoksa hata dön
        if (!userModules.Any())
        {
            return Result<List<ModuleListDto>>.Failure("Size atanmış hiçbir modül bulunamadı. Lütfen modül kaydı için talepte bulunun.");
        }

        var moduleDtos = new List<ModuleListDto>();

        foreach (var userModule in userModules)
        {
            var module = await _unitOfWork.Modules.GetByIdAsync(userModule.ModuleId, cancellationToken);
            if (module == null || !module.IsActive) continue;

            var moduleWithTrainings = await _unitOfWork.Modules.GetModuleWithTrainingsAsync(module.Id, cancellationToken);

            // ⚠️ ÖNEMLİ: Sadece yayınlanmış eğitimleri say
            var publishedTrainingsCount = moduleWithTrainings?.Trainings.Count(t => t.IsPublished) ?? 0;

            moduleDtos.Add(new ModuleListDto
            {
                Id = module.Id,
                Name = module.Name,
                Description = module.Description,
                OrderIndex = module.OrderIndex,
                IsActive = module.IsActive,
                ImagePath = module.ImagePath,
                EstimatedDurationMinutes = module.EstimatedDurationMinutes,
                TotalTrainings = publishedTrainingsCount,
                CompletionPercentage = userModule.CompletionPercentage,
                IsCompleted = userModule.IsCompleted
            });
        }

        return Result<List<ModuleListDto>>.Success(
            moduleDtos.OrderBy(m => m.OrderIndex).ToList(),
            "Modüller başarıyla getirildi.");
    }
}
