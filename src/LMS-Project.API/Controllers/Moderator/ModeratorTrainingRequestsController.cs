using LMS_Project.Application.Features.TrainingRequests.Commands;
using LMS_Project.Application.Features.TrainingRequests.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers.Moderator;

[ApiController]
[Route("api/moderator/trainingrequests")]
[Authorize(Roles = "Moderator,Admin")]
public class ModeratorTrainingRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModeratorTrainingRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Bekleyen eğitim taleplerini listeler
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var query = new GetPendingTrainingRequestsQuery();
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Eğitim talebini onaylar ve UserTraining oluşturur
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveRequest(Guid id, [FromBody] ApproveTrainingRequestDto? dto)
    {
        var command = new ApproveTrainingRequestCommand(id, dto?.ReviewNote);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Eğitim talebini reddeder
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectRequest(Guid id, [FromBody] RejectTrainingRequestDto dto)
    {
        var command = new RejectTrainingRequestCommand(id, dto.ReviewNote);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

public class ApproveTrainingRequestDto
{
    public string? ReviewNote { get; set; }
}

public class RejectTrainingRequestDto
{
    public string ReviewNote { get; set; } = string.Empty;
}
