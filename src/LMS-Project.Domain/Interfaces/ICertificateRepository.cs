using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface ICertificateRepository : IRepository<Certificate>
{
    Task<IEnumerable<Certificate>> GetCertificatesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Certificate?> GetCertificateByVerificationCodeAsync(string verificationCode, CancellationToken cancellationToken = default);
    Task<Certificate?> GetCertificateByNumberAsync(string certificateNumber, CancellationToken cancellationToken = default);
    Task<bool> HasUserCertificateForModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default);
    Task<bool> HasUserCertificateForTrainingAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Certificate>> GetCertificatesByUserAndTrainingAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Certificate>> GetCertificatesByUserAndModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default);
}
