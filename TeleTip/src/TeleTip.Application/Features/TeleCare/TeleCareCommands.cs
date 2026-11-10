using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Entities;

namespace MyProject.Application.Features.TeleCare;

public sealed record CreatePatientCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? PhoneNumber,
    string? Email) : ICommand<Guid>;

public sealed record CreateDoctorCommand(
    string FirstName,
    string LastName,
    string Specialization,
    string LicenseNumber,
    string? PhoneNumber,
    string? Email) : ICommand<Guid>;

public sealed record CreateConsultationCommand(
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAt) : ICommand<Guid>;

public sealed record CreateIoTDeviceCommand(
    Guid PatientId,
    string Name,
    string SerialNumber,
    DeviceType Type) : ICommand<Guid>;

public sealed record CreateHealthMeasurementCommand(
    Guid PatientId,
    Guid? DeviceId,
    MeasurementType Type,
    decimal Value,
    string Unit,
    DateTime MeasuredAt) : ICommand<Guid>;

public sealed record CreatePrescriptionCommand(
    Guid PatientId,
    Guid DoctorId,
    Guid? ConsultationId,
    string MedicationName,
    string Dosage,
    string Instructions,
    DateTime? ExpiresAt) : ICommand<Guid>;

public sealed record CreateMedicalReportCommand(
    Guid PatientId,
    Guid DoctorId,
    Guid? ConsultationId,
    string Title,
    string Content) : ICommand<Guid>;

public sealed record CreateAiAnalysisCommand(
    Guid PatientId,
    Guid? ConsultationId,
    AnalysisType Type,
    string AnalysisResult,
    decimal? RiskScore) : ICommand<Guid>;

public sealed record CreateNotificationCommand(
    Guid PatientId,
    Guid? DoctorId,
    NotificationType Type,
    string Title,
    string Message) : ICommand<Guid>;

public sealed record CreateChatMessageCommand(
    Guid ConsultationId,
    string SenderId,
    ConsultationParticipantRole SenderRole,
    string Message) : ICommand<Guid>;
