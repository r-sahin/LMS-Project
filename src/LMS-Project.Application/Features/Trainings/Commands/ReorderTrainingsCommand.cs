using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Trainings.Commands;

public record ReorderTrainingsCommand(List<TrainingOrderDto> Trainings) : IRequest<Result>;
public record TrainingOrderDto(Guid Id, int OrderIndex);

public class ReorderTrainingsCommandHandler : IRequestHandler<ReorderTrainingsCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReorderTrainingsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ReorderTrainingsCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Trainings)
        {
            var training = await _unitOfWork.Trainings.GetByIdAsync(item.Id, cancellationToken);
            if (training != null)
            {
                training.OrderIndex = item.OrderIndex;
                training.UpdatedBy = _currentUserService.UserId.ToString();
                training.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Trainings.Update(training);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Eğitimler başarıyla yeniden sıralandı.");
    }
}
