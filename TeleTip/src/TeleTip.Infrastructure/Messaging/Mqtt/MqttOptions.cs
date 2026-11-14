namespace MyProject.Infrastructure.Messaging.Mqtt;

public sealed class MqttOptions
{
    public const string SectionName = "Mqtt";

    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 1883;
    public string ClientId { get; set; } = "teletip-api";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string TopicFilter { get; set; } = "teletip/tenants/+/patients/+/measurements";
}
