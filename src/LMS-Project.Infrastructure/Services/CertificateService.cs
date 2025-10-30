using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;

namespace LMS_Project.Infrastructure.Services;

public class CertificateService : ICertificateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProgressService _progressService;

    public CertificateService(
        IUnitOfWork unitOfWork,
        IProgressService progressService)
    {
        _unitOfWork = unitOfWork;
        _progressService = progressService;
    }

    public async Task<Result<CertificateDto>> GenerateModuleCertificateAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken cancellationToken = default)
    {
        // 1. Modülün tamamlanıp tamamlanmadığını kontrol et
        var isModuleCompletedResult = await _progressService.IsModuleCompletedAsync(userId, moduleId, cancellationToken);
        if (!isModuleCompletedResult.IsSuccess || !isModuleCompletedResult.Data)
        {
            return Result<CertificateDto>.Failure("Modül henüz tamamlanmadı. Sertifika oluşturulamaz.");
        }

        // 2. Daha önce sertifika verilmiş mi kontrol et
        var hasExistingCertificate = await _unitOfWork.Certificates
            .HasUserCertificateForModuleAsync(userId, moduleId, cancellationToken);

        if (hasExistingCertificate)
        {
            return Result<CertificateDto>.Failure("Bu modül için zaten bir sertifikanız bulunmaktadır.");
        }

        // 3. Kullanıcı ve modül bilgilerini getir
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        var module = await _unitOfWork.Modules.GetByIdAsync(moduleId, cancellationToken);

        if (user == null || module == null)
        {
            return Result<CertificateDto>.Failure("Kullanıcı veya modül bulunamadı.");
        }

        // 4. Sertifika oluştur
        var certificateNumber = GenerateCertificateNumber("MOD");
        var verificationCode = GenerateVerificationCode();

        var certificate = new Certificate
        {
            UserId = userId,
            ModuleId = moduleId,
            TrainingId = null,
            CertificateType = "Module",
            CertificateNumber = certificateNumber,
            IssuedDate = DateTime.UtcNow,
            PdfFilePath = $"/certificates/{certificateNumber}.pdf", // PDF oluşturma servisi ile entegre edilebilir
            IsRevoked = false,
            VerificationCode = verificationCode,
            CreatedBy = userId.ToString()
        };

        await _unitOfWork.Certificates.AddAsync(certificate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. DTO'ya dönüştür
        var certificateDto = new CertificateDto
        {
            Id = certificate.Id,
            UserId = certificate.UserId,
            UserFullName = user.FullName,
            ModuleId = certificate.ModuleId,
            ModuleName = module.Name,
            TrainingId = null,
            TrainingName = null,
            CertificateType = certificate.CertificateType,
            CertificateNumber = certificate.CertificateNumber,
            IssuedDate = certificate.IssuedDate,
            PdfFilePath = certificate.PdfFilePath,
            IsRevoked = certificate.IsRevoked,
            VerificationCode = certificate.VerificationCode
        };

        return Result<CertificateDto>.Success(certificateDto, "Modül sertifikası başarıyla oluşturuldu.");
    }

    public async Task<Result<CertificateDto>> GenerateTrainingCertificateAsync(
        Guid userId,
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        // 1. Eğitimin tamamlanıp tamamlanmadığını kontrol et
        var isTrainingCompletedResult = await _progressService.IsTrainingCompletedAsync(userId, trainingId, cancellationToken);
        if (!isTrainingCompletedResult.IsSuccess || !isTrainingCompletedResult.Data)
        {
            return Result<CertificateDto>.Failure("Eğitim henüz tamamlanmadı. Sertifika oluşturulamaz.");
        }

        // 2. Daha önce sertifika verilmiş mi kontrol et
        var hasExistingCertificate = await _unitOfWork.Certificates
            .HasUserCertificateForTrainingAsync(userId, trainingId, cancellationToken);

        if (hasExistingCertificate)
        {
            return Result<CertificateDto>.Failure("Bu eğitim için zaten bir sertifikanız bulunmaktadır.");
        }

        // 3. Kullanıcı ve eğitim bilgilerini getir
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        var training = await _unitOfWork.Trainings.GetByIdAsync(trainingId, cancellationToken);

        if (user == null || training == null)
        {
            return Result<CertificateDto>.Failure("Kullanıcı veya eğitim bulunamadı.");
        }

        // 4. Sertifika oluştur
        var certificateNumber = GenerateCertificateNumber("TRN");
        var verificationCode = GenerateVerificationCode();

        var certificate = new Certificate
        {
            UserId = userId,
            ModuleId = null,
            TrainingId = trainingId,
            CertificateType = "Training",
            CertificateNumber = certificateNumber,
            IssuedDate = DateTime.UtcNow,
            PdfFilePath = $"/certificates/{certificateNumber}.pdf",
            IsRevoked = false,
            VerificationCode = verificationCode,
            CreatedBy = userId.ToString()
        };

        await _unitOfWork.Certificates.AddAsync(certificate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. DTO'ya dönüştür
        var certificateDto = new CertificateDto
        {
            Id = certificate.Id,
            UserId = certificate.UserId,
            UserFullName = user.FullName,
            ModuleId = null,
            ModuleName = null,
            TrainingId = certificate.TrainingId,
            TrainingName = training.Name,
            CertificateType = certificate.CertificateType,
            CertificateNumber = certificate.CertificateNumber,
            IssuedDate = certificate.IssuedDate,
            PdfFilePath = certificate.PdfFilePath,
            IsRevoked = certificate.IsRevoked,
            VerificationCode = certificate.VerificationCode
        };

        return Result<CertificateDto>.Success(certificateDto, "Eğitim sertifikası başarıyla oluşturuldu.");
    }

    public async Task<Result<CertificateDto>> GetCertificateByIdAsync(
        Guid certificateId,
        CancellationToken cancellationToken = default)
    {
        var certificate = await _unitOfWork.Certificates.GetByIdWithIncludesAsync(
            certificateId,
            cancellationToken,
            c => c.User,
            c => c.Module!,
            c => c.Training!);

        if (certificate == null)
        {
            return Result<CertificateDto>.Failure("Sertifika bulunamadı.");
        }

        var certificateDto = new CertificateDto
        {
            Id = certificate.Id,
            UserId = certificate.UserId,
            UserFullName = certificate.User.FullName,
            ModuleId = certificate.ModuleId,
            ModuleName = certificate.Module?.Name,
            TrainingId = certificate.TrainingId,
            TrainingName = certificate.Training?.Name,
            CertificateType = certificate.CertificateType,
            CertificateNumber = certificate.CertificateNumber,
            IssuedDate = certificate.IssuedDate,
            PdfFilePath = certificate.PdfFilePath,
            IsRevoked = certificate.IsRevoked,
            RevokedDate = certificate.RevokedDate,
            RevokedReason = certificate.RevokedReason,
            VerificationCode = certificate.VerificationCode
        };

        return Result<CertificateDto>.Success(certificateDto);
    }

    public async Task<Result<CertificateDto>> GetCertificateByVerificationCodeAsync(
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        var certificate = await _unitOfWork.Certificates.GetCertificateByVerificationCodeAsync(verificationCode, cancellationToken);

        if (certificate == null)
        {
            return Result<CertificateDto>.Failure("Sertifika bulunamadı.");
        }

        return await GetCertificateByIdAsync(certificate.Id, cancellationToken);
    }

    public async Task<Result<List<CertificateListDto>>> GetUserCertificatesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var certificates = await _unitOfWork.Certificates.GetCertificatesByUserIdAsync(userId, cancellationToken);

        var certificateDtos = certificates.Select(c => new CertificateListDto
        {
            Id = c.Id,
            CertificateType = c.CertificateType,
            CertificateNumber = c.CertificateNumber,
            EntityName = c.CertificateType == "Module"
                ? c.Module?.Name ?? "Bilinmiyor"
                : c.Training?.Name ?? "Bilinmiyor",
            IssuedDate = c.IssuedDate,
            IsRevoked = c.IsRevoked
        }).ToList();

        return Result<List<CertificateListDto>>.Success(certificateDtos, "Sertifikalar başarıyla getirildi.");
    }

    public async Task<Result> RevokeCertificateAsync(
        Guid certificateId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var certificate = await _unitOfWork.Certificates.GetByIdAsync(certificateId, cancellationToken);

        if (certificate == null)
        {
            return Result.Failure("Sertifika bulunamadı.");
        }

        if (certificate.IsRevoked)
        {
            return Result.Failure("Sertifika zaten iptal edilmiş.");
        }

        certificate.IsRevoked = true;
        certificate.RevokedDate = DateTime.UtcNow;
        certificate.RevokedReason = reason;
        certificate.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Certificates.Update(certificate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Sertifika başarıyla iptal edildi.");
    }

    public async Task<Result<byte[]>> DownloadCertificatePdfAsync(
        Guid certificateId,
        CancellationToken cancellationToken = default)
    {
        var certificate = await _unitOfWork.Certificates.GetByIdAsync(certificateId, cancellationToken);

        if (certificate == null)
        {
            return Result<byte[]>.Failure("Sertifika bulunamadı.");
        }

        if (certificate.IsRevoked)
        {
            return Result<byte[]>.Failure("Bu sertifika iptal edilmiştir.");
        }

        // TODO: Gerçek PDF oluşturma servisi ile entegre edilmeli
        // Şimdilik boş byte array döndürüyoruz
        return Result<byte[]>.Failure("PDF oluşturma servisi henüz aktif değil.");
    }

    private string GenerateCertificateNumber(string prefix)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"{prefix}-{timestamp}-{random}";
    }

    private string GenerateVerificationCode()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
    }
}
