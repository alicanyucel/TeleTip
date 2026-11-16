using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Application.Abstractions.Persistence
{
    public interface ITeleCareRepository
    {
        Task AddAsync(Entity entity, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : Entity;
        Task<IReadOnlyList<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : Entity;
        Task<IReadOnlyList<HealthMeasurement>> GetRecentHealthMeasurementsAsync(
            Guid patientId,
            int limit,
            CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
