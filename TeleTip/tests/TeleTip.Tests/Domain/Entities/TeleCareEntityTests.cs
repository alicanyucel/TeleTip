using FluentAssertions;
using MyProject.Domain.Entities;

namespace MyProject.Tests.Domain.Entities;

public sealed class TeleCareEntityTests
{
    [Fact]
    public void Patient_Create_ShouldTrimNamesAndSucceed()
    {
        var result = Patient.Create(" Ali ", " Yılmaz ", new DateOnly(1990, 1, 1), "555", "ali@example.com");

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("Ali");
        result.Value.LastName.Should().Be("Yılmaz");
    }

    [Theory]
    [InlineData("", "Last")]
    [InlineData("First", "")]
    public void Patient_Create_RequiredNameMissing_ShouldFail(string firstName, string lastName)
    {
        var result = Patient.Create(firstName, lastName, new DateOnly(1990, 1, 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Validation");
    }

    [Fact]
    public void Patient_Create_FutureDateOfBirth_ShouldFail()
    {
        var result = Patient.Create("Ali", "Yılmaz", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain(nameof(Patient.DateOfBirth));
    }

    [Fact]
    public void Doctor_Create_ShouldTrimFieldsAndSucceed()
    {
        var result = Doctor.Create(" Ayşe ", " Kaya ", "Cardiology", " LIC-1 ");

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("Ayşe");
        result.Value.Specialization.Should().Be("Cardiology");
        result.Value.LicenseNumber.Should().Be("LIC-1");
    }

    [Theory]
    [InlineData("", "Last", "Specialization", "License")]
    [InlineData("First", "Last", "", "License")]
    [InlineData("First", "Last", "Specialization", "")]
    public void Doctor_Create_RequiredFieldMissing_ShouldFail(
        string firstName, string lastName, string specialization, string licenseNumber)
    {
        var result = Doctor.Create(firstName, lastName, specialization, licenseNumber);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Consultation_Lifecycle_ShouldTransitionCorrectly()
    {
        var result = TelemedicineConsultation.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        var consultation = result.Value;

        consultation.Status.Should().Be(ConsultationStatus.Scheduled);
        consultation.Start();
        consultation.Status.Should().Be(ConsultationStatus.InProgress);
        consultation.StartedAt.Should().NotBeNull();
        consultation.Complete("Completed");
        consultation.Status.Should().Be(ConsultationStatus.Completed);
        consultation.Notes.Should().Be("Completed");
        consultation.EndedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Consultation_Create_MissingParticipant_ShouldFail(bool emptyPatient, bool emptyDoctor)
    {
        var result = TelemedicineConsultation.Create(
            emptyPatient ? Guid.Empty : Guid.NewGuid(),
            emptyDoctor ? Guid.Empty : Guid.NewGuid(),
            DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void IoTDevice_CreateAndDeactivate_ShouldWork()
    {
        var result = IoTDevice.Create(Guid.NewGuid(), " Smart Watch ", " SN-1 ", DeviceType.SmartWatch);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Smart Watch");
        result.Value.SerialNumber.Should().Be("SN-1");
        result.Value.IsActive.Should().BeTrue();
        result.Value.Deactivate();
        result.Value.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.1)]
    public void HealthMeasurement_NegativeValue_ShouldFail(decimal value)
    {
        var result = HealthMeasurement.Create(Guid.NewGuid(), MeasurementType.HeartRate, value, "bpm", DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void HealthMeasurement_Create_ShouldTrimUnit()
    {
        var result = HealthMeasurement.Create(Guid.NewGuid(), MeasurementType.BloodGlucose, 98, " mg/dL ", DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Unit.Should().Be("mg/dL");
    }

    [Fact]
    public void Prescription_Create_ShouldSetIssuedAtAndFields()
    {
        var result = Prescription.Create(Guid.NewGuid(), Guid.NewGuid(), "Medicine", "1x1", "After meal");

        result.IsSuccess.Should().BeTrue();
        result.Value.MedicationName.Should().Be("Medicine");
        result.Value.IssuedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MedicalReport_Create_EmptyContent_ShouldFail()
    {
        var result = MedicalReport.Create(Guid.NewGuid(), Guid.NewGuid(), "Report", " ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain(nameof(MedicalReport.Content));
    }

    [Theory]
    [InlineData(0.69, false)]
    [InlineData(0.70, true)]
    [InlineData(1.0, true)]
    public void AiAnalysis_Create_ShouldSetDoctorReviewFromRisk(decimal riskScore, bool requiresReview)
    {
        var result = AiAnalysis.Create(Guid.NewGuid(), AnalysisType.RiskDetection, "Risk result", riskScore);

        result.IsSuccess.Should().BeTrue();
        result.Value.RequiresDoctorReview.Should().Be(requiresReview);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void AiAnalysis_Create_RiskOutsideRange_ShouldFail(decimal riskScore)
    {
        var result = AiAnalysis.Create(Guid.NewGuid(), AnalysisType.General, "Result", riskScore);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Notification_MarkAsRead_ShouldSetReadState()
    {
        var result = Notification.Create(Guid.NewGuid(), NotificationType.AiAlert, "Alert", "Review required");

        result.Value.IsRead.Should().BeFalse();
        result.Value.MarkAsRead();
        result.Value.IsRead.Should().BeTrue();
        result.Value.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public void ChatMessage_Create_ShouldTrimMessage()
    {
        var result = ChatMessage.Create(Guid.NewGuid(), "user-1", ConsultationParticipantRole.Patient, " Hello ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Be("Hello");
        result.Value.SenderRole.Should().Be(ConsultationParticipantRole.Patient);
    }

    [Fact]
    public void ChatMessage_Create_EmptyMessage_ShouldFail()
    {
        var result = ChatMessage.Create(Guid.NewGuid(), "user-1", ConsultationParticipantRole.Patient, " ");

        result.IsFailure.Should().BeTrue();
    }
}
