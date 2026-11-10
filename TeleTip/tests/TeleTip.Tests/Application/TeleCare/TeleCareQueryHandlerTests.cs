using FluentAssertions;
using MyProject.Application.Abstractions.Persistence;
using MyProject.Application.Features.TeleCare;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using NSubstitute;

namespace MyProject.Tests.Application.TeleCare;

public sealed class TeleCareQueryHandlerTests
{
    private readonly ITeleCareRepository _repository = Substitute.For<ITeleCareRepository>();

    [Fact]
    public async Task GetPatients_ShouldReturnRepositoryResults()
    {
        var patient = Patient.Create("Ali", "Yılmaz", new DateOnly(1990, 1, 1)).Value;
        _repository.GetAllAsync<Patient>(Arg.Any<CancellationToken>()).Returns([patient]);

        var result = await new GetPatientsQueryHandler(_repository).Handle(new(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.Should().Be(patient);
    }

    [Fact]
    public async Task GetDoctors_ShouldReturnRepositoryResults()
    {
        var doctor = Doctor.Create("Ayşe", "Kaya", "Cardiology", "LIC-1").Value;
        _repository.GetAllAsync<Doctor>(Arg.Any<CancellationToken>()).Returns([doctor]);

        var result = await new GetDoctorsQueryHandler(_repository).Handle(new(), CancellationToken.None);

        result.Value.Should().ContainSingle().Which.Should().Be(doctor);
    }

    [Fact]
    public async Task GetConsultations_ShouldReturnRepositoryResults()
    {
        var consultation = TelemedicineConsultation.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow).Value;
        _repository.GetAllAsync<TelemedicineConsultation>(Arg.Any<CancellationToken>()).Returns([consultation]);

        var result = await new GetConsultationsQueryHandler(_repository).Handle(new(), CancellationToken.None);

        result.Value.Should().ContainSingle().Which.Should().Be(consultation);
    }

    [Fact]
    public async Task GetIoTDevices_ShouldReturnRepositoryResults()
    {
        var device = IoTDevice.Create(Guid.NewGuid(), "Watch", "SN-1", DeviceType.SmartWatch).Value;
        _repository.GetAllAsync<IoTDevice>(Arg.Any<CancellationToken>()).Returns([device]);

        var result = await new GetIoTDevicesQueryHandler(_repository).Handle(new(), CancellationToken.None);

        result.Value.Should().ContainSingle().Which.Should().Be(device);
    }

    [Fact]
    public async Task GetHealthMeasurements_ShouldReturnRepositoryResults()
    {
        var measurement = HealthMeasurement.Create(Guid.NewGuid(), MeasurementType.HeartRate, 70, "bpm", DateTime.UtcNow).Value;
        _repository.GetAllAsync<HealthMeasurement>(Arg.Any<CancellationToken>()).Returns([measurement]);

        var result = await new GetHealthMeasurementsQueryHandler(_repository).Handle(new(), CancellationToken.None);

        result.Value.Should().ContainSingle().Which.Should().Be(measurement);
    }

    [Fact]
    public async Task GetPrescriptions_ShouldReturnRepositoryResults()
    {
        var prescription = Prescription.Create(Guid.NewGuid(), Guid.NewGuid(), "Medicine", "1x1", "After meal").Value;
        _repository.GetAllAsync<Prescription>(Arg.Any<CancellationToken>()).Returns([prescription]);

        var result = await new GetPrescriptionsQueryHandler(_repository).Handle(new(), CancellationToken.None);

        result.Value.Should().ContainSingle().Which.Should().Be(prescription);
    }

    [Fact]
    public async Task GetMedicalReports_ShouldReturnRepositoryResults()
    {
        var report = MedicalReport.Create(Guid.NewGuid(), Guid.NewGuid(), "Report", "Content").Value;
        _repository.GetAllAsync<MedicalReport>(Arg.Any<CancellationToken>()).Returns([report]);

        var result = await new GetMedicalReportsQueryHandler(_repository).Handle(new(), CancellationToken.None);

        result.Value.Should().ContainSingle().Which.Should().Be(report);
    }

    [Fact]
    public async Task GetAiAnalyses_ShouldReturnRepositoryResults()
    {
        var analysis = AiAnalysis.Create(Guid.NewGuid(), AnalysisType.General, "Result", 0.2m).Value;
        _repository.GetAllAsync<AiAnalysis>(Arg.Any<CancellationToken>()).Returns([analysis]);

        var result = await new GetAiAnalysesQueryHandler(_repository).Handle(new(), CancellationToken.None);

        result.Value.Should().ContainSingle().Which.Should().Be(analysis);
    }

    [Fact]
    public async Task GetNotifications_ShouldReturnRepositoryResults()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationType.General, "Title", "Message").Value;
        _repository.GetAllAsync<Notification>(Arg.Any<CancellationToken>()).Returns([notification]);

        var result = await new GetNotificationsQueryHandler(_repository).Handle(new(), CancellationToken.None);

        result.Value.Should().ContainSingle().Which.Should().Be(notification);
    }

    [Fact]
    public async Task GetChatMessages_ShouldReturnRepositoryResults()
    {
        var chatMessage = ChatMessage.Create(Guid.NewGuid(), "user-1", ConsultationParticipantRole.Patient, "Hello").Value;
        _repository.GetAllAsync<ChatMessage>(Arg.Any<CancellationToken>()).Returns([chatMessage]);

        var result = await new GetChatMessagesQueryHandler(_repository).Handle(new(), CancellationToken.None);

        result.Value.Should().ContainSingle().Which.Should().Be(chatMessage);
    }

    [Fact]
    public async Task GetPatientById_WhenFound_ShouldReturnPatient()
    {
        var patient = Patient.Create("Ali", "Yılmaz", new DateOnly(1990, 1, 1)).Value;
        _repository.GetByIdAsync<Patient>(patient.Id, Arg.Any<CancellationToken>()).Returns(patient);

        var result = await new GetPatientByIdQueryHandler(_repository).Handle(new(patient.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(patient);
    }

    [Fact]
    public async Task GetPatientById_WhenMissing_ShouldReturnNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync<Patient>(id, Arg.Any<CancellationToken>()).Returns((Patient?)null);

        var result = await new GetPatientByIdQueryHandler(_repository).Handle(new(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Patient.NotFound");
    }

    [Fact]
    public async Task GetChatMessageById_WhenMissing_ShouldReturnNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync<ChatMessage>(id, Arg.Any<CancellationToken>()).Returns((ChatMessage?)null);

        var result = await new GetChatMessageByIdQueryHandler(_repository).Handle(new(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ChatMessage.NotFound");
    }
}
