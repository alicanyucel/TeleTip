using MyProject.Domain.Abstractions;

namespace MyProject.Application.Abstractions.Persistence;

public interface ITeleCareRepository
{
    Task AddAsync(Entity entity, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : Entity;
    Task<IReadOnlyList<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : Entity;
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
