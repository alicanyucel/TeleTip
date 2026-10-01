using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Repositories
{
    public interface IProductRepository : IRepository<Entities.Product>
    {
        Task<IEnumerable<Entities.Product>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    }
}
