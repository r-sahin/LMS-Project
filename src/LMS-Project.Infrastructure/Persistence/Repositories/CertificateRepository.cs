using LMS_Project.Domain.Entities;
using LMS_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS_Project.Infrastructure.Persistence.Repositories;

public class CertificateRepository : Repository<Certificate>, ICertificateRepository
{
    public CertificateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Certificate>> GetCertificatesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => !c.IsDeleted && c.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<Certificate?> GetCertificateByVerificationCodeAsync(string verificationCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => !c.IsDeleted).FirstOrDefaultAsync(c => c.VerificationCode == verificationCode, cancellationToken);
    }

    public async Task<Certificate?> GetCertificateByNumberAsync(string certificateNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => !c.IsDeleted).FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber, cancellationToken);
    }

    public async Task<bool> HasUserCertificateForModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => !c.IsDeleted).AnyAsync(c => c.UserId == userId && c.ModuleId == moduleId && !c.IsRevoked, cancellationToken);
    }

    public async Task<bool> HasUserCertificateForTrainingAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => !c.IsDeleted).AnyAsync(c => c.UserId == userId && c.TrainingId == trainingId && !c.IsRevoked, cancellationToken);
    }

    public async Task<IEnumerable<Certificate>> GetCertificatesByUserAndTrainingAsync(Guid userId, Guid trainingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => !c.IsDeleted && c.UserId == userId && c.TrainingId == trainingId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Certificate>> GetCertificatesByUserAndModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => !c.IsDeleted && c.UserId == userId && c.ModuleId == moduleId).ToListAsync(cancellationToken);
    }
}
