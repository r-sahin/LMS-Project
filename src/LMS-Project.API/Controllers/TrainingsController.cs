using LMS_Project.Application.Features.Trainings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TrainingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrainingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Kullanıcının kayıtlı olduğu tüm eğitimleri getirir (UserTraining tablosundan)
    /// İlerleme bilgileri, tamamlanma durumu ve sıradaki erişilebilir SubTopic ile birlikte
    /// </summary>
    [HttpGet("my-trainings")]
    public async Task<IActionResult> GetMyTrainings()
    {
        var result = await _mediator.Send(new GetMyTrainingsQuery());

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTrainingByIdQuery(id));

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
