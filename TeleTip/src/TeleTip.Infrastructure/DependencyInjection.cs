using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyProject.Application.Abstractions.Ai;
using MyProject.Application.Abstractions.Blockchain;
using MyProject.Application.Abstractions.Cache;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Abstractions.MultiTenancy;
using MyProject.Application.Abstractions.Persistence;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Repositories;
using MyProject.Infrastructure.Ai;
using MyProject.Infrastructure.Blockchain;
using MyProject.Infrastructure.Cache;
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

            services.AddOptions<OllamaOptions>()
                .Bind(configuration.GetSection(OllamaOptions.SectionName))
                .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                    "Ollama:BaseUrl must be an absolute HTTP or HTTPS URL.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Model), "Ollama:Model is required.")
                .Validate(options => options.TimeoutSeconds is > 0 and <= 600, "Ollama:TimeoutSeconds must be between 1 and 600.")
                .Validate(options => options.MaxInputMeasurements is > 0 and <= 100, "Ollama:MaxInputMeasurements must be between 1 and 100.")
                .ValidateOnStart();
            services.AddHttpClient<IAiAnalysisEngine, OllamaAiAnalysisEngine>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

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
