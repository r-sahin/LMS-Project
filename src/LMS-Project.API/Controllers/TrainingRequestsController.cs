using LMS_Project.Application.Features.TrainingRequests.Commands;
using LMS_Project.Application.Features.TrainingRequests.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student,Admin,Moderator")]
public class TrainingRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrainingRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Eğitim erişimi için talepte bulunur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RequestTrainingEnrollment([FromBody] RequestTrainingEnrollmentDto dto)
    {
        var command = new RequestTrainingEnrollmentCommand(dto.TrainingId, dto.RequestReason);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Kendi eğitim taleplerini görüntüler
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        var query = new GetMyTrainingRequestsQuery();
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

public class RequestTrainingEnrollmentDto
{
    public Guid TrainingId { get; set; }
    public string RequestReason { get; set; } = string.Empty;
}
