using LMS_Project.Application.Features.ModuleRequests.Commands;
using LMS_Project.Application.Features.ModuleRequests.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers.Moderator;

[Authorize(Roles = "Moderator,Admin")]
[ApiController]
[Route("api/moderator/[controller]")]
public class ModeratorModuleRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModeratorModuleRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Bekleyen tüm talepleri getir (Moderator/Admin)
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var result = await _mediator.Send(new GetPendingModuleRequestsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Talebi onayla (Moderator/Admin)
    /// </summary>
    [HttpPost("{requestId}/approve")]
    public async Task<IActionResult> ApproveRequest(
        Guid requestId,
        [FromBody] ApproveRequestDto dto)
    {
        var command = new ApproveModuleRequestCommand(requestId, dto.ReviewNote);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Talebi reddet (Moderator/Admin)
    /// </summary>
    [HttpPost("{requestId}/reject")]
    public async Task<IActionResult> RejectRequest(
        Guid requestId,
        [FromBody] RejectRequestDto dto)
    {
        var command = new RejectModuleRequestCommand(requestId, dto.RejectReason);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

public record ApproveRequestDto(string? ReviewNote);
public record RejectRequestDto(string RejectReason);
