using LMS_Project.Application.Features.Progress.Commands;
using LMS_Project.Application.Features.Progress.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProgressController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Kullanıcının bir alt başlıktaki ilerlemesini günceller
    /// Minimum süre kontrolü yapar ve gerekirse tamamlar
    /// </summary>
    [HttpPost("update")]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Bir modüldeki genel ilerleme özetini getirir
    /// </summary>
    [HttpGet("summary/{moduleId}")]
    public async Task<IActionResult> GetProgressSummary(Guid moduleId)
    {
        var result = await _mediator.Send(new GetProgressSummaryQuery(moduleId));

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Bir eğitimdeki erişilebilir sonraki alt başlığı getirir
    /// Sıralı ilerleme kontrolü yapar
    /// </summary>
    [HttpGet("next-subtopic/{trainingId}")]
    public async Task<IActionResult> GetNextAvailableSubTopic(Guid trainingId)
    {
        var result = await _mediator.Send(new GetNextAvailableSubTopicQuery(trainingId));

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
