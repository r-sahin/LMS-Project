using LMS_Project.Application.Features.ModuleRequests.Commands;
using LMS_Project.Application.Features.ModuleRequests.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ModuleRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModuleRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Modül kaydı için talep oluştur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RequestEnrollment([FromBody] RequestModuleEnrollmentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Kullanıcının kendi taleplerini getir
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        var result = await _mediator.Send(new GetMyModuleRequestsQuery());
        return Ok(result);
    }
}
