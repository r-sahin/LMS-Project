using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace LMS_Project.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _frontendUrl;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
        _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@lms.com";
        _fromName = _configuration["Email:FromName"] ?? "LMS Platform";
        _frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";
    }

    public async Task<Result> SendPasswordResetEmailAsync(
        string toEmail,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_frontendUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(toEmail)}";

        var subject = "Şifre Sıfırlama Talebi";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Şifre Sıfırlama</h1>
        </div>
        <div class='content'>
            <h2>Merhaba,</h2>
            <p>Hesabınız için şifre sıfırlama talebinde bulundunuz.</p>
            <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
            <p style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Şifre Sıfırla</a>
            </p>
            <p><strong>Not:</strong> Bu bağlantı 1 saat süreyle geçerlidir.</p>
            <p>Eğer bu talebi siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 LMS Platform. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body, true, cancellationToken);
    }

    public async Task<Result> SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var subject = "LMS Platformuna Hoş Geldiniz!";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #2196F3; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Hoş Geldiniz!</h1>
        </div>
        <div class='content'>
            <h2>Merhaba {userName},</h2>
            <p>LMS Platformuna katıldığınız için teşekkür ederiz!</p>
            <p>Hesabınız başarıyla oluşturuldu. Artık eğitimlerimize erişebilir ve öğrenme yolculuğunuza başlayabilirsiniz.</p>
            <p style='text-align: center;'>
                <a href='{_frontendUrl}' class='button'>Platforma Git</a>
            </p>
            <p>İyi öğrenmeler!</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 LMS Platform. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body, true, cancellationToken);
    }

    public async Task<Result> SendCertificateEmailAsync(
        string toEmail,
        string userName,
        string certificateId,
        CancellationToken cancellationToken = default)
    {
        var certificateUrl = $"{_frontendUrl}/certificates/{certificateId}";

        var subject = "Tebrikler! Sertifikanızı Kazandınız";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #FF9800; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Tebrikler!</h1>
        </div>
        <div class='content'>
            <h2>Sayın {userName},</h2>
            <p>Eğitiminizi başarıyla tamamladınız ve sertifikanızı kazandınız!</p>
            <p>Gösterdiğiniz özveri ve çaba için sizi kutluyoruz.</p>
            <p style='text-align: center;'>
                <a href='{certificateUrl}' class='button'>Sertifikamı Görüntüle</a>
            </p>
            <p>Sertifikanızı indirip paylaşabilirsiniz.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 LMS Platform. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body, true, cancellationToken);
    }

    public async Task<Result> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // SMTP ayarları yapılmamışsa, sadece loglayalım (test/development için)
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                Console.WriteLine($"[EMAIL] To: {toEmail}, Subject: {subject}");
                Console.WriteLine($"[EMAIL] Body: {body}");
                return Result.Success("Email gönderimi simüle edildi (SMTP yapılandırması bulunamadı).");
            }

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            return Result.Success("Email başarıyla gönderildi.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Email gönderilirken hata oluştu: {ex.Message}");
        }
    }
}
