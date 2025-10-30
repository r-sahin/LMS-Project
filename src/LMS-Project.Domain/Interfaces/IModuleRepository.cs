using LMS_Project.Domain.Entities;

namespace LMS_Project.Domain.Interfaces;

public interface IModuleRepository : IRepository<Module>
{
    Task<Module?> GetModuleWithTrainingsAsync(Guid moduleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Module>> GetActiveModulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Module>> GetModulesWithTrainingsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsModuleCompletedByUserAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken = default);
}
