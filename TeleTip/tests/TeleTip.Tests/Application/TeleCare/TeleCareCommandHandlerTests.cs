using FluentAssertions;
using MyProject.Application.Abstractions.Persistence;
using MyProject.Application.Features.TeleCare;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using NSubstitute;

namespace MyProject.Tests.Application.TeleCare;

public sealed class TeleCareCommandHandlerTests
{
    private readonly ITeleCareRepository _repository = Substitute.For<ITeleCareRepository>();

    [Fact]
    public async Task CreatePatient_ValidCommand_ShouldPersistAndReturnId()
    {
        var handler = new CreatePatientCommandHandler(_repository);

        var result = await handler.Handle(
            new CreatePatientCommand("Ali", "Yılmaz", new DateOnly(1990, 1, 1), null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await AssertPersistedAsync<Patient>();
    }

    [Fact]
    public async Task CreatePatient_InvalidCommand_ShouldNotPersist()
    {
        var handler = new CreatePatientCommandHandler(_repository);

        var result = await handler.Handle(
            new CreatePatientCommand("", "Yılmaz", new DateOnly(1990, 1, 1), null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateDoctor_ValidCommand_ShouldPersist()
    {
        var handler = new CreateDoctorCommandHandler(_repository);

        var result = await handler.Handle(
            new CreateDoctorCommand("Ayşe", "Kaya", "Cardiology", "LIC-1", null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await AssertPersistedAsync<Doctor>();
    }

    [Fact]
    public async Task CreateConsultation_ValidCommand_ShouldPersist()
    {
        var handler = new CreateConsultationCommandHandler(_repository);

        var result = await handler.Handle(
            new CreateConsultationCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await AssertPersistedAsync<TelemedicineConsultation>();
    }

    [Fact]
    public async Task CreateIoTDevice_ValidCommand_ShouldPersist()
    {
        var handler = new CreateIoTDeviceCommandHandler(_repository);

        var result = await handler.Handle(
            new CreateIoTDeviceCommand(Guid.NewGuid(), "Watch", "SN-1", DeviceType.SmartWatch),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await AssertPersistedAsync<IoTDevice>();
    }

    [Fact]
    public async Task CreateHealthMeasurement_ValidCommand_ShouldPersist()
    {
        var handler = new CreateHealthMeasurementCommandHandler(_repository);

        var result = await handler.Handle(
            new CreateHealthMeasurementCommand(Guid.NewGuid(), null, MeasurementType.HeartRate, 72, "bpm", DateTime.UtcNow),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await AssertPersistedAsync<HealthMeasurement>();
    }

    [Fact]
    public async Task CreatePrescription_ValidCommand_ShouldPersist()
    {
        var handler = new CreatePrescriptionCommandHandler(_repository);

        var result = await handler.Handle(
            new CreatePrescriptionCommand(Guid.NewGuid(), Guid.NewGuid(), null, "Medicine", "1x1", "After meal", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await AssertPersistedAsync<Prescription>();
    }

    [Fact]
    public async Task CreateMedicalReport_ValidCommand_ShouldPersist()
    {
        var handler = new CreateMedicalReportCommandHandler(_repository);

        var result = await handler.Handle(
            new CreateMedicalReportCommand(Guid.NewGuid(), Guid.NewGuid(), null, "Report", "Content"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await AssertPersistedAsync<MedicalReport>();
    }

    [Fact]
    public async Task CreateAiAnalysis_ValidCommand_ShouldPersist()
    {
        var handler = new CreateAiAnalysisCommandHandler(_repository);

        var result = await handler.Handle(
            new CreateAiAnalysisCommand(Guid.NewGuid(), null, AnalysisType.RiskDetection, "Risk detected", 0.8m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await AssertPersistedAsync<AiAnalysis>();
    }

    [Fact]
    public async Task CreateNotification_ValidCommand_ShouldPersist()
    {
        var handler = new CreateNotificationCommandHandler(_repository);

        var result = await handler.Handle(
            new CreateNotificationCommand(Guid.NewGuid(), null, NotificationType.AiAlert, "Alert", "Review required"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await AssertPersistedAsync<Notification>();
    }

    [Fact]
    public async Task CreateChatMessage_ValidCommand_ShouldPersist()
    {
        var handler = new CreateChatMessageCommandHandler(_repository);

        var result = await handler.Handle(
            new CreateChatMessageCommand(Guid.NewGuid(), "user-1", ConsultationParticipantRole.Patient, "Hello"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await AssertPersistedAsync<ChatMessage>();
    }

    [Fact]
    public async Task CreateAiAnalysis_InvalidRisk_ShouldNotPersist()
    {
        var handler = new CreateAiAnalysisCommandHandler(_repository);

        var result = await handler.Handle(
            new CreateAiAnalysisCommand(Guid.NewGuid(), null, AnalysisType.General, "Result", 1.1m),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private async Task AssertPersistedAsync<TEntity>() where TEntity : Entity
    {
        await _repository.Received(1).AddAsync(Arg.Any<TEntity>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
