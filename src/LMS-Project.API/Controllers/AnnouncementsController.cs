using LMS_Project.Application.Features.Announcements.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnnouncementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Aktif duyuruları getir (kullanıcılar için)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var result = await _mediator.Send(new GetAnnouncementsQuery(OnlyActive: true));
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
