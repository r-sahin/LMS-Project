using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Modules.Commands;

public record ReorderModulesCommand(List<ModuleOrderDto> Modules) : IRequest<Result>;

public record ModuleOrderDto(Guid Id, int OrderIndex);

public class ReorderModulesCommandHandler : IRequestHandler<ReorderModulesCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReorderModulesCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ReorderModulesCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Modules)
        {
            var module = await _unitOfWork.Modules.GetByIdAsync(item.Id, cancellationToken);
            if (module != null)
            {
                module.OrderIndex = item.OrderIndex;
                module.UpdatedBy = _currentUserService.UserId.ToString();
                module.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Modules.Update(module);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Modüller başarıyla yeniden sıralandı.");
    }
}
