using LMS_Project.Domain.Common;

namespace LMS_Project.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Şifre sıfırlama emaili gönderir
    /// </summary>
    Task<Result> SendPasswordResetEmailAsync(
        string toEmail,
        string resetToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hoş geldiniz emaili gönderir
    /// </summary>
    Task<Result> SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sertifika kazanma bildirimi emaili gönderir
    /// </summary>
    Task<Result> SendCertificateEmailAsync(
        string toEmail,
        string userName,
        string certificateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Genel email gönderir
    /// </summary>
    Task<Result> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default);
}
