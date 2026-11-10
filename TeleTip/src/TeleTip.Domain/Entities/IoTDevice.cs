using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class IoTDevice : Entity
{
    private IoTDevice() { }

    public Guid PatientId { get; private set; }
    public string Name { get; private set; } = default!;
    public string SerialNumber { get; private set; } = default!;
    public DeviceType Type { get; private set; }
    public bool IsActive { get; private set; }

    public static Result<IoTDevice> Create(
        Guid patientId,
        string name,
        string serialNumber,
        DeviceType type)
    {
        if (patientId == Guid.Empty)
            return Result.Failure<IoTDevice>(Error.Validation(nameof(PatientId), "Patient is required."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<IoTDevice>(Error.Validation(nameof(Name), "Device name cannot be empty."));

        if (string.IsNullOrWhiteSpace(serialNumber))
            return Result.Failure<IoTDevice>(Error.Validation(nameof(SerialNumber), "Serial number cannot be empty."));

        return Result.Success(new IoTDevice
        {
            PatientId = patientId,
            Name = name.Trim(),
            SerialNumber = serialNumber.Trim(),
            Type = type,
            IsActive = true
        });
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum DeviceType
{
    SmartWatch,
    BloodPressureMonitor,
    Glucometer
}
