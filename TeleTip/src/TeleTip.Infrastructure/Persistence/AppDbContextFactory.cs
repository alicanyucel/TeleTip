using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MyProject.Application.Abstractions.MultiTenancy;
using MyProject.Domain.Entities;

namespace MyProject.Infrastructure.Persistence
{
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(),
                    "..", "MyProject.WebAPI"))
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("SqlServer"));

            // Design-time için stub implementasyonlar
            var publisher = new NoOpPublisher();
            var tenantService = new NoOpTenantService();

            return new AppDbContext(optionsBuilder.Options, publisher, tenantService);
        }
    }

    // Design-time stub — çalışma zamanında gerçek implementasyonlar inject edilir
    file sealed class NoOpPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;
    }

    file sealed class NoOpTenantService : ITenantService
    {
        public string? GetCurrentTenantId() => null;
    }
}
