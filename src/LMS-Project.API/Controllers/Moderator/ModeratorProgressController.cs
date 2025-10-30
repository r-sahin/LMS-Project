using LMS_Project.Application.Features.Trainings.Commands;
using LMS_Project.Application.Features.Modules.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers.Moderator;

[Authorize(Roles = "Moderator,Admin")]
[ApiController]
[Route("api/moderator/[controller]")]
public class ModeratorProgressController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModeratorProgressController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Kullanıcının bir eğitimini sıfırlar
    /// UserTraining kaydı kalır, sadece progress ve sertifikalar silinir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="trainingId">Eğitim ID</param>
    /// <param name="dto">Sıfırlama nedeni</param>
    [HttpPost("training/reset")]
    public async Task<IActionResult> ResetUserTraining([FromBody] ResetUserTrainingDto dto)
    {
        var command = new ResetUserTrainingCommand(dto.UserId, dto.TrainingId, dto.ResetReason);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Kullanıcının bir modülünü ve içindeki tüm eğitimleri sıfırlar
    /// UserModule ve UserTraining kayıtları kalır, sadece progress ve sertifikalar silinir
    /// </summary>
    /// <param name="dto">Sıfırlama bilgileri</param>
    [HttpPost("module/reset")]
    public async Task<IActionResult> ResetUserModule([FromBody] ResetUserModuleDto dto)
    {
        var command = new ResetUserModuleCommand(dto.UserId, dto.ModuleId, dto.ResetReason);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

public class ResetUserTrainingDto
{
    public Guid UserId { get; set; }
    public Guid TrainingId { get; set; }
    public string ResetReason { get; set; } = string.Empty;
}

public class ResetUserModuleDto
{
    public Guid UserId { get; set; }
    public Guid ModuleId { get; set; }
    public string ResetReason { get; set; } = string.Empty;
}
