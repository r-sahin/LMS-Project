using LMS_Project.Application.Features.Trainings.Commands;
using LMS_Project.Application.Features.Trainings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/[controller]")]
public class AdminTrainingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminTrainingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateTrainingCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateTrainingCommand command)
    {
        if (id != command.Id) return BadRequest("ID uyuşmazlığı.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteTrainingCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderTrainingsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Eğitimi yayınlar (Sadece Admin) - Alt başlıkların toplamı eğitim süresine eşit olmalı
    /// </summary>
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var result = await _mediator.Send(new PublishTrainingCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Eğitimi yayından kaldırır (Sadece Admin)
    /// </summary>
    [HttpPost("{id}/unpublish")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var result = await _mediator.Send(new UnpublishTrainingCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Eğitimin süre bilgilerini getirir (Toplam süre, kullanılan süre, kalan süre, yayınlanabilir mi?)
    /// </summary>
    [HttpGet("{id}/duration-info")]
    public async Task<IActionResult> GetDurationInfo(Guid id)
    {
        var result = await _mediator.Send(new GetTrainingDurationInfoQuery(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Bir modüle ait tüm eğitimleri listeler
    /// </summary>
    /// <param name="moduleId">Modül ID</param>
    /// <param name="includeUnpublished">Yayınlanmamış eğitimleri de göster (varsayılan: true)</param>
    [HttpGet("module/{moduleId}")]
    public async Task<IActionResult> GetByModuleId(Guid moduleId, [FromQuery] bool includeUnpublished = true)
    {
        var query = new GetTrainingsByModuleIdQuery(moduleId, includeUnpublished);
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
