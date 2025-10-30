using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.Trainings.Commands;

public record DeleteTrainingCommand(Guid Id) : IRequest<Result>;

public class DeleteTrainingCommandHandler : IRequestHandler<DeleteTrainingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public DeleteTrainingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result> Handle(DeleteTrainingCommand request, CancellationToken cancellationToken)
    {
        var training = await _unitOfWork.Trainings.GetByIdAsync(request.Id, cancellationToken);
        if (training == null) return Result.Failure("Eğitim bulunamadı.");

        training.IsDeleted = true;
        training.DeletedDate = DateTime.UtcNow;
        training.DeletedBy = _currentUserService.UserId.ToString();

        _unitOfWork.Trainings.Update(training);

        await _fileService.DeleteTrainingFilesAsync(training.ModuleId, request.Id, cancellationToken);

        if (!string.IsNullOrEmpty(training.ThumbnailPath))
            await _fileService.DeleteFileAsync(training.ThumbnailPath, cancellationToken);
        if (!string.IsNullOrEmpty(training.VideoIntroPath))
            await _fileService.DeleteFileAsync(training.VideoIntroPath, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Eğitim başarıyla silindi.");
    }
}
