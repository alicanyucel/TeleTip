using MyProject.Domain.Entities;

namespace MyProject.Infrastructure.Messaging.Mqtt;

public sealed record WatchMeasurementMessage(
    Guid DeviceId,
    MeasurementType Type,
    decimal Value,
    string Unit,
    DateTime MeasuredAt,
    string? Source = "Samsung Galaxy Watch 8");
