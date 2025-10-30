using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace LMS_Project.Application.Features.Modules.Commands;

public record UpdateModuleDurationCommand(
    Guid Id,
    int EstimatedDurationMinutes) : IRequest<Result>;

public class UpdateModuleDurationCommandValidator : AbstractValidator<UpdateModuleDurationCommand>
{
    public UpdateModuleDurationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Modül ID gereklidir.");

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Tahmini süre 0'dan büyük olmalıdır.");
    }
}

public class UpdateModuleDurationCommandHandler : IRequestHandler<UpdateModuleDurationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateModuleDurationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateModuleDurationCommand request, CancellationToken cancellationToken)
    {
        var module = await _unitOfWork.Modules.GetByIdAsync(request.Id, cancellationToken);
        if (module == null)
        {
            return Result.Failure("Modül bulunamadı.");
        }

        // ⚠️ UYARI: Mevcut eğitimlerin toplam süresini kontrol et
        var trainings = await _unitOfWork.Trainings.GetTrainingsByModuleIdAsync(request.Id, cancellationToken);
        var currentTrainingsTotalSeconds = trainings.Sum(t => t.TotalDurationSeconds);
        var newModuleTotalSeconds = request.EstimatedDurationMinutes * 60;

        if (currentTrainingsTotalSeconds > newModuleTotalSeconds)
        {
            var excessSeconds = currentTrainingsTotalSeconds - newModuleTotalSeconds;
            var excessMinutes = excessSeconds / 60;
            var excessSecondsRemainder = excessSeconds % 60;

            return Result.Failure(
                $"Modül süresi mevcut eğitimlerin toplam süresinden az olamaz! " +
                $"Mevcut eğitimler toplamı: {currentTrainingsTotalSeconds} saniye ({currentTrainingsTotalSeconds / 60} dakika). " +
                $"Yeni modül süresi: {newModuleTotalSeconds} saniye ({request.EstimatedDurationMinutes} dakika). " +
                $"Fazla: {excessMinutes} dakika {excessSecondsRemainder} saniye. " +
                $"Önce eğitimleri güncelleyin veya silin.");
        }

        module.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        module.UpdatedBy = _currentUserService.UserId.ToString();
        module.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Modules.Update(module);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success($"Modül süresi {request.EstimatedDurationMinutes} dakika olarak güncellendi.");
    }
}
