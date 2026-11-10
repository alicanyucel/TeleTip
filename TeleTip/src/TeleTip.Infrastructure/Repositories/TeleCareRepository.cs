using Microsoft.EntityFrameworkCore;
using MyProject.Application.Abstractions.Persistence;
using MyProject.Domain.Abstractions;
using MyProject.Infrastructure.Persistence;

namespace MyProject.Infrastructure.Repositories;

internal sealed class TeleCareRepository(AppDbContext context) : ITeleCareRepository
{
    public Task AddAsync(Entity entity, CancellationToken cancellationToken = default)
    {
        context.Add(entity);
        return Task.CompletedTask;
    }

    public Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : Entity
        => context.Set<T>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(CancellationToken cancellationToken = default)
        where T : Entity
        => await context.Set<T>().AsNoTracking().ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
