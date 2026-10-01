using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;
using MyProject.Infrastructure.Persistence;

namespace MyProject.Infrastructure.Repositories
{
    internal sealed class ProductRepository(AppDbContext context) : Repository<Product>(context), IProductRepository
    {
        public async Task<IEnumerable<Product>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
            => await DbSet.Where(p => p.TenantId == tenantId).ToListAsync(cancellationToken);
    }
}
