using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyProject.Application.Abstractions.Blockchain;
using MyProject.Application.Abstractions.Cache;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Abstractions.MultiTenancy;
using MyProject.Application.Abstractions.Persistence;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Repositories;
using MyProject.Infrastructure.Cache;
using MyProject.Infrastructure.Blockchain;
using MyProject.Infrastructure.Messaging;
using MyProject.Infrastructure.Messaging.Consumers;
using MyProject.Infrastructure.Messaging.Mqtt;
using MyProject.Infrastructure.Messaging.Sagas;
using MyProject.Infrastructure.MultiTenancy;
using MyProject.Infrastructure.Persistence;
using MyProject.Infrastructure.Repositories;
using StackExchange.Redis;

namespace MyProject.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // EF Core
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddScoped<ITeleCareRepository, TeleCareRepository>();

            // EVM blockchain notarization
            services.Configure<BlockchainOptions>(configuration.GetSection(BlockchainOptions.SectionName));
            services.AddScoped<IHealthRecordNotarizer, NethereumHealthRecordNotarizer>();

            // Repositories
            services.AddScoped<IProductRepository, ProductRepository>();

            // Multi-Tenancy
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantService, TenantService>();

            // Redis Cache
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
            services.AddScoped<ICacheService, RedisCacheService>();

            // MassTransit + RabbitMQ + Saga
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ProductCreatedConsumer>();

                x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(configuration.GetConnectionString("RabbitMQ"));

                    cfg.ReceiveEndpoint("product-created", e =>
                        e.ConfigureConsumer<ProductCreatedConsumer>(ctx));

                    cfg.ReceiveEndpoint("order-saga", e =>
                        e.ConfigureSaga<OrderState>(ctx));
                });
            });

            services.AddScoped<IEventBus, EventBus>();

            // Samsung Galaxy Watch 8 telemetry over MQTT
            services.Configure<MqttOptions>(configuration.GetSection(MqttOptions.SectionName));
            services.AddHostedService<GalaxyWatchMqttConsumer>();

            return services;
        }
    }
}
