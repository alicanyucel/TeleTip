using MediatR;
using MyProject.Application.Abstractions.Persistence;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Application.Features.TeleCare;

internal sealed class CreatePatientCommandHandler(ITeleCareRepository repository)
    : IRequestHandler<CreatePatientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var result = Patient.Create(request.FirstName, request.LastName, request.DateOfBirth, request.PhoneNumber, request.Email);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await repository.AddAsync(result.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}

internal sealed class CreateDoctorCommandHandler(ITeleCareRepository repository)
    : IRequestHandler<CreateDoctorCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var result = Doctor.Create(request.FirstName, request.LastName, request.Specialization, request.LicenseNumber, request.PhoneNumber, request.Email);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await repository.AddAsync(result.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}

internal sealed class CreateConsultationCommandHandler(ITeleCareRepository repository)
    : IRequestHandler<CreateConsultationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateConsultationCommand request, CancellationToken cancellationToken)
    {
        var result = TelemedicineConsultation.Create(request.PatientId, request.DoctorId, request.ScheduledAt);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await repository.AddAsync(result.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}

internal sealed class CreateIoTDeviceCommandHandler(ITeleCareRepository repository)
    : IRequestHandler<CreateIoTDeviceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateIoTDeviceCommand request, CancellationToken cancellationToken)
    {
        var result = IoTDevice.Create(request.PatientId, request.Name, request.SerialNumber, request.Type);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await repository.AddAsync(result.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}

internal sealed class CreateHealthMeasurementCommandHandler(ITeleCareRepository repository)
    : IRequestHandler<CreateHealthMeasurementCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateHealthMeasurementCommand request, CancellationToken cancellationToken)
    {
        var result = HealthMeasurement.Create(request.PatientId, request.Type, request.Value, request.Unit, request.MeasuredAt, request.DeviceId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await repository.AddAsync(result.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}

internal sealed class CreatePrescriptionCommandHandler(ITeleCareRepository repository)
    : IRequestHandler<CreatePrescriptionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var result = Prescription.Create(request.PatientId, request.DoctorId, request.MedicationName, request.Dosage, request.Instructions, request.ConsultationId, request.ExpiresAt);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await repository.AddAsync(result.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}

internal sealed class CreateMedicalReportCommandHandler(ITeleCareRepository repository)
    : IRequestHandler<CreateMedicalReportCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMedicalReportCommand request, CancellationToken cancellationToken)
    {
        var result = MedicalReport.Create(request.PatientId, request.DoctorId, request.Title, request.Content, request.ConsultationId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await repository.AddAsync(result.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}

internal sealed class CreateAiAnalysisCommandHandler(ITeleCareRepository repository)
    : IRequestHandler<CreateAiAnalysisCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAiAnalysisCommand request, CancellationToken cancellationToken)
    {
        var result = AiAnalysis.Create(request.PatientId, request.Type, request.AnalysisResult, request.RiskScore, request.ConsultationId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await repository.AddAsync(result.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}

internal sealed class CreateNotificationCommandHandler(ITeleCareRepository repository)
    : IRequestHandler<CreateNotificationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var result = Notification.Create(request.PatientId, request.Type, request.Title, request.Message, request.DoctorId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await repository.AddAsync(result.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}

internal sealed class CreateChatMessageCommandHandler(ITeleCareRepository repository)
    : IRequestHandler<CreateChatMessageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateChatMessageCommand request, CancellationToken cancellationToken)
    {
        var result = ChatMessage.Create(request.ConsultationId, request.SenderId, request.SenderRole, request.Message);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await repository.AddAsync(result.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}
