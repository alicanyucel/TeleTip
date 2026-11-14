using System.Buffers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.Persistence;

namespace MyProject.Infrastructure.Messaging.Mqtt;

internal sealed class GalaxyWatchMqttConsumer(
    IOptions<MqttOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<GalaxyWatchMqttConsumer> logger) : BackgroundService
{
    private readonly MqttOptions _options = options.Value;
    private IMqttClient? _client;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += HandleMessageAsync;

        var builder = new MqttClientOptionsBuilder()
            .WithClientId($"{_options.ClientId}-{Guid.NewGuid():N}")
            .WithTcpServer(_options.Host, _options.Port)
            .WithCleanSession();

        if (!string.IsNullOrWhiteSpace(_options.Username))
            builder.WithCredentials(_options.Username, _options.Password);

        var mqttOptions = builder.Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_client.IsConnected)
                {
                    await _client.ConnectAsync(mqttOptions, stoppingToken);
                    await _client.SubscribeAsync(
                        new MqttTopicFilterBuilder().WithTopic(_options.TopicFilter).Build(),
                        stoppingToken);
                    logger.LogInformation("MQTT telemetry consumer connected to {Host}:{Port}", _options.Host, _options.Port);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "MQTT connection failed. Retrying in 10 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        if (_client.IsConnected)
            await _client.DisconnectAsync(cancellationToken: CancellationToken.None);
    }

    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
    {
        if (!TryParseTopic(eventArgs.ApplicationMessage.Topic, out var tenantId, out var patientId))
        {
            logger.LogWarning("Ignoring MQTT message with invalid topic {Topic}", eventArgs.ApplicationMessage.Topic);
            return;
        }

        try
        {
            var payload = eventArgs.ApplicationMessage.Payload;
            var message = JsonSerializer.Deserialize<WatchMeasurementMessage>(payload.ToArray());
            if (message is null || message.DeviceId == Guid.Empty || message.Value < 0 || string.IsNullOrWhiteSpace(message.Unit))
            {
                logger.LogWarning("Ignoring invalid Galaxy Watch measurement payload.");
                return;
            }

            var measurement = HealthMeasurement.Create(
                patientId,
                message.Type,
                message.Value,
                message.Unit,
                message.MeasuredAt,
                message.DeviceId);

            if (measurement.IsFailure)
            {
                logger.LogWarning("Rejected health measurement: {Error}", measurement.Error.Description);
                return;
            }

            measurement.Value.SetTenant(tenantId);
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.HealthMeasurements.Add(measurement.Value);
            await db.SaveChangesAsync();
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Ignoring malformed Galaxy Watch MQTT payload.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to persist Galaxy Watch measurement.");
        }
    }

    private static bool TryParseTopic(string topic, out string tenantId, out Guid patientId)
    {
        tenantId = string.Empty;
        patientId = Guid.Empty;
        var segments = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length != 6 ||
            !segments[0].Equals("teletip", StringComparison.OrdinalIgnoreCase) ||
            !segments[1].Equals("tenants", StringComparison.OrdinalIgnoreCase) ||
            !segments[3].Equals("patients", StringComparison.OrdinalIgnoreCase) ||
            !segments[5].Equals("measurements", StringComparison.OrdinalIgnoreCase) ||
            !Guid.TryParse(segments[4], out patientId))
        {
            return false;
        }

        tenantId = segments[2];
        return !string.IsNullOrWhiteSpace(tenantId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client?.IsConnected == true)
            await _client.DisconnectAsync(cancellationToken: cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
