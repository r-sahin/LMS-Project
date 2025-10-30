using LMS_Project.Application.DTOs;
using LMS_Project.Domain.Common;

namespace LMS_Project.Application.Interfaces;

public interface ICertificateService
{
    Task<Result<CertificateDto>> GenerateModuleCertificateAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken cancellationToken = default);

    Task<Result<CertificateDto>> GenerateTrainingCertificateAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default);

    Task<Result<CertificateDto>> GetCertificateByIdAsync(
        Guid certificateId,
        CancellationToken cancellationToken = default);

    Task<Result<CertificateDto>> GetCertificateByVerificationCodeAsync(
        string verificationCode,
        CancellationToken cancellationToken = default);

    Task<Result<List<CertificateListDto>>> GetUserCertificatesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeCertificateAsync(
        Guid certificateId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<Result<byte[]>> DownloadCertificatePdfAsync(
        Guid certificateId,
        CancellationToken cancellationToken = default);
}
