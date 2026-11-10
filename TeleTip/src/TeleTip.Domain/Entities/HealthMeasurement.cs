using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class HealthMeasurement : Entity
{
    private HealthMeasurement() { }

    public Guid PatientId { get; private set; }
    public Guid? DeviceId { get; private set; }
    public MeasurementType Type { get; private set; }
    public decimal Value { get; private set; }
    public string Unit { get; private set; } = default!;
    public DateTime MeasuredAt { get; private set; }

    public static Result<HealthMeasurement> Create(
        Guid patientId,
        MeasurementType type,
        decimal value,
        string unit,
        DateTime measuredAt,
        Guid? deviceId = null)
    {
        if (patientId == Guid.Empty)
            return Result.Failure<HealthMeasurement>(Error.Validation(nameof(PatientId), "Patient is required."));

        if (string.IsNullOrWhiteSpace(unit))
            return Result.Failure<HealthMeasurement>(Error.Validation(nameof(Unit), "Unit cannot be empty."));

        if (value < 0)
            return Result.Failure<HealthMeasurement>(Error.Validation(nameof(Value), "Value cannot be negative."));

        return Result.Success(new HealthMeasurement
        {
            PatientId = patientId,
            DeviceId = deviceId,
            Type = type,
            Value = value,
            Unit = unit.Trim(),
            MeasuredAt = measuredAt
        });
    }
}

public enum MeasurementType
{
    HeartRate,
    BloodPressure,
    BloodGlucose,
    OxygenSaturation,
    BodyTemperature
}
