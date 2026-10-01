using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Abstractions;
using MyProject.Infrastructure.Persistence;

namespace MyProject.Infrastructure.Repositories
{
    internal abstract class Repository<T>(AppDbContext context) : IRepository<T>
        where T : Entity
    {
        protected readonly AppDbContext Context = context;
        protected readonly DbSet<T> DbSet = context.Set<T>();

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
            => await DbSet.ToListAsync(cancellationToken);

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await DbSet.AddAsync(entity, cancellationToken);

        public void Update(T entity)
            => DbSet.Update(entity);

        public void Delete(T entity)
            => DbSet.Remove(entity);
    }
}
