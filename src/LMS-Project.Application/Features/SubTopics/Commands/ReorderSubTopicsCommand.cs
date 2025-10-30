using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Interfaces;
using MediatR;

namespace LMS_Project.Application.Features.SubTopics.Commands;

public record ReorderSubTopicsCommand(List<SubTopicOrderDto> SubTopics) : IRequest<Result>;
public record SubTopicOrderDto(Guid Id, int OrderIndex);

public class ReorderSubTopicsCommandHandler : IRequestHandler<ReorderSubTopicsCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReorderSubTopicsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ReorderSubTopicsCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.SubTopics)
        {
            var subTopic = await _unitOfWork.SubTopics.GetByIdAsync(item.Id, cancellationToken);
            if (subTopic != null)
            {
                subTopic.OrderIndex = item.OrderIndex;
                subTopic.UpdatedBy = _currentUserService.UserId.ToString();
                subTopic.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.SubTopics.Update(subTopic);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Alt başlıklar başarıyla yeniden sıralandı.");
    }
}
