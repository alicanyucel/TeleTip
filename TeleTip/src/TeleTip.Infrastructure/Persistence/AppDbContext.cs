using MediatR;
using Microsoft.EntityFrameworkCore;
using MyProject.Application.Abstractions.MultiTenancy;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Infrastructure.Persistence
{
    public sealed class AppDbContext(
        DbContextOptions<AppDbContext> options,
        IPublisher publisher,
        ITenantService tenantService) : DbContext(options), IUnitOfWork
    {
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Multi-tenant global query filter
            var tenantId = tenantService.GetCurrentTenantId();
            if (!string.IsNullOrEmpty(tenantId))
            {
                modelBuilder.Entity<Product>().HasQueryFilter(p => p.TenantId == tenantId);
            }

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Publish domain events before saving
            var domainEvents = ChangeTracker.Entries<Entity>()
                .Select(e => e.Entity)
                .SelectMany(e =>
                {
                    var events = e.DomainEvents.ToList();
                    e.ClearDomainEvents();
                    return events;
                })
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in domainEvents)
                await publisher.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
