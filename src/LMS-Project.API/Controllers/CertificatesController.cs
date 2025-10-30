using LMS_Project.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Project.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certificateService;
    private readonly ICurrentUserService _currentUserService;

    public CertificatesController(
        ICertificateService certificateService,
        ICurrentUserService currentUserService)
    {
        _certificateService = certificateService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Tamamlanan modül için sertifika oluşturur
    /// Modüldeki TÜM eğitimler tamamlanmış olmalı
    /// </summary>
    [HttpPost("module/{moduleId}")]
    public async Task<IActionResult> GenerateModuleCertificate(Guid moduleId)
    {
        var userId = _currentUserService.UserId;
        var result = await _certificateService.GenerateModuleCertificateAsync(userId, moduleId);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Tamamlanan eğitim için sertifika oluşturur
    /// Eğitimdeki TÜM alt başlıklar tamamlanmış olmalı
    /// </summary>
    [HttpPost("training/{trainingId}")]
    public async Task<IActionResult> GenerateTrainingCertificate(Guid trainingId)
    {
        var userId = _currentUserService.UserId;
        var result = await _certificateService.GenerateTrainingCertificateAsync(userId, trainingId);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının tüm sertifikalarını getirir
    /// </summary>
    [HttpGet("my-certificates")]
    public async Task<IActionResult> GetMyCertificates()
    {
        var userId = _currentUserService.UserId;
        var result = await _certificateService.GetUserCertificatesAsync(userId);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Doğrulama kodu ile sertifika sorgular (Public endpoint)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("verify/{verificationCode}")]
    public async Task<IActionResult> VerifyCertificate(string verificationCode)
    {
        var result = await _certificateService.GetCertificateByVerificationCodeAsync(verificationCode);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Sertifika detayını getirir
    /// </summary>
    [HttpGet("{certificateId}")]
    public async Task<IActionResult> GetCertificate(Guid certificateId)
    {
        var result = await _certificateService.GetCertificateByIdAsync(certificateId);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
