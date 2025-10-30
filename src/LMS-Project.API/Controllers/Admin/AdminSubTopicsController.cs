using LMS_Project.Application.Features.SubTopics.Commands;
using LMS_Project.Application.Features.SubTopics.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/[controller]")]
public class AdminSubTopicsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminSubTopicsController> _logger;

    public AdminSubTopicsController(IMediator mediator, ILogger<AdminSubTopicsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Alt başlık oluştur - ZIP dosyası otomatik olarak ayıklanır
    /// </summary>
    [HttpPost]
    [DisableRequestSizeLimit] // Büyük ZIP dosyaları için
    public async Task<IActionResult> Create([FromForm] CreateSubTopicCommand command)
    {
        try
        {
            _logger.LogInformation("SubTopic oluşturma başladı. TrainingId: {TrainingId}, Name: {Name}", command.TrainingId, command.Name);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("SubTopic başarıyla oluşturuldu. ID: {Id}", result.Data);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("SubTopic oluşturulamadı. Hata: {Message}", result.Message);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubTopic oluşturulurken exception oluştu");
            return StatusCode(500, new
            {
                IsSuccess = false,
                Message = "SubTopic oluşturulurken beklenmeyen bir hata oluştu",
                Errors = new[] { ex.Message },
                Data = Guid.Empty
            });
        }
    }

    /// <summary>
    /// Alt başlık güncelle - Yeni ZIP yüklenirse eski silinir
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateSubTopicCommand command)
    {
        if (id != command.Id) return BadRequest("ID uyuşmazlığı.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Alt başlık sil - ZIP ve extract edilen dosyalar da silinir
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteSubTopicCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Alt başlıkları yeniden sırala
    /// </summary>
    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderSubTopicsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Bir eğitime ait tüm alt başlıkları listeler
    /// </summary>
    [HttpGet("training/{trainingId}")]
    public async Task<IActionResult> GetByTrainingId(Guid trainingId)
    {
        try
        {
            _logger.LogInformation("Eğitim alt başlıkları getiriliyor. TrainingId: {TrainingId}", trainingId);

            var query = new GetSubTopicsByTrainingIdQuery(trainingId);
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                _logger.LogInformation("{Count} alt başlık bulundu", result.Data?.Count ?? 0);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Alt başlık bulunamadı. Hata: {Message}", result.Message);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Alt başlıklar getirilirken hata oluştu");
            return StatusCode(500, new
            {
                IsSuccess = false,
                Message = "Alt başlıklar getirilirken hata oluştu",
                Errors = new[] { ex.Message }
            });
        }
    }
}
