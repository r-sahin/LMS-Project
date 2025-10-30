using LMS_Project.Application.Features.Announcements.Commands;
using LMS_Project.Application.Features.Announcements.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/[controller]")]
public class AdminAnnouncementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAnnouncementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Duyuru oluştur (resim ile birlikte)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateAnnouncementCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Duyuru güncelle
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateAnnouncementCommand command)
    {
        if (id != command.Id) return BadRequest("ID uyuşmazlığı.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Duyuru sil
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteAnnouncementCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Tüm duyuruları getir (aktif ve pasif)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = false)
    {
        var result = await _mediator.Send(new GetAnnouncementsQuery(onlyActive));
        return Ok(result);
    }

    /// <summary>
    /// ID'ye göre duyuru getir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAnnouncementByIdQuery(id));
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}
