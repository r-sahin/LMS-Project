using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace LMS_Project.Application.Features.Trainings.Commands;

public record UnpublishTrainingCommand(Guid TrainingId) : IRequest<Result>;

public class UnpublishTrainingCommandValidator : AbstractValidator<UnpublishTrainingCommand>
{
    public UnpublishTrainingCommandValidator()
    {
        RuleFor(x => x.TrainingId)
            .NotEmpty().WithMessage("Eğitim ID gereklidir.");
    }
}

public class UnpublishTrainingCommandHandler : IRequestHandler<UnpublishTrainingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UnpublishTrainingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UnpublishTrainingCommand request, CancellationToken cancellationToken)
    {
        var training = await _unitOfWork.Trainings.GetByIdAsync(request.TrainingId, cancellationToken);
        if (training == null)
        {
            return Result.Failure("Eğitim bulunamadı.");
        }

        if (!training.IsPublished)
        {
            return Result.Failure("Eğitim zaten yayından kaldırılmış.");
        }

        training.IsPublished = false;
        training.PublishedDate = null;
        training.PublishedBy = null;
        training.UpdatedBy = _currentUserService.UserId.ToString();
        training.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Trainings.Update(training);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Eğitim yayından kaldırıldı.");
    }
}
