using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.SubTopics.Commands;

public record DeleteSubTopicCommand(Guid Id) : IRequest<Result>;

public class DeleteSubTopicCommandHandler : IRequestHandler<DeleteSubTopicCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public DeleteSubTopicCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result> Handle(DeleteSubTopicCommand request, CancellationToken cancellationToken)
    {
        var subTopic = await _unitOfWork.SubTopics.GetByIdWithIncludesAsync(request.Id, cancellationToken, st => st.Training);
        if (subTopic == null) return Result.Failure("Alt başlık bulunamadı.");

        subTopic.IsDeleted = true;
        subTopic.DeletedDate = DateTime.UtcNow;
        subTopic.DeletedBy = _currentUserService.UserId.ToString();

        _unitOfWork.SubTopics.Update(subTopic);

        // Fiziksel dosyaları sil (ZIP + Extract edilen dosyalar)
        await _fileService.DeleteSubTopicFilesAsync(
            subTopic.Training.ModuleId,
            subTopic.TrainingId,
            subTopic.Id,
            cancellationToken);

        if (!string.IsNullOrEmpty(subTopic.ThumbnailPath))
            await _fileService.DeleteFileAsync(subTopic.ThumbnailPath, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Alt başlık ve dosyaları başarıyla silindi.");
    }
}
