using LMS_Project.Application.Features.Modules.Commands;
using LMS_Project.Application.Features.Modules.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/[controller]")]
public class AdminModulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminModulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateModuleCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateModuleDto dto)
    {
        var command = new UpdateModuleCommand(
            id,
            dto.Name,
            dto.Description,
            dto.EstimatedDurationMinutes,
            dto.IsActive,
            dto.ImageFile);

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Modül süresini günceller (PATCH - Sadece süre)
    /// </summary>
    [HttpPatch("{id}/duration")]
    public async Task<IActionResult> UpdateDuration(Guid id, [FromBody] UpdateModuleDurationDto dto)
    {
        var command = new UpdateModuleDurationCommand(id, dto.EstimatedDurationMinutes);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteModuleCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderModulesCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Modülün süre bilgilerini getirir (Toplam süre, kullanılan süre, kalan süre)
    /// </summary>
    [HttpGet("{id}/duration-info")]
    public async Task<IActionResult> GetDurationInfo(Guid id)
    {
        var result = await _mediator.Send(new GetModuleDurationInfoQuery(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Modülü yayınlar (IsPublished = true)
    /// En az bir yayınlanmış eğitim olmalı ve toplam süre eşit olmalı
    /// </summary>
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var result = await _mediator.Send(new PublishModuleCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Modülü yayından kaldırır (IsPublished = false)
    /// </summary>
    [HttpPost("{id}/unpublish")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var result = await _mediator.Send(new UnpublishModuleCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

/// <summary>
/// Update işlemi için DTO (FromForm için gerekli)
/// </summary>
public class UpdateModuleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool IsActive { get; set; }
    public IFormFile? ImageFile { get; set; }
}

/// <summary>
/// Sadece süre güncellemesi için DTO
/// </summary>
public class UpdateModuleDurationDto
{
    public int EstimatedDurationMinutes { get; set; }
}
