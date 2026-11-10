using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class Prescription : Entity
{
    private Prescription() { }

    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid? ConsultationId { get; private set; }
    public string MedicationName { get; private set; } = default!;
    public string Dosage { get; private set; } = default!;
    public string Instructions { get; private set; } = default!;
    public DateTime IssuedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public static Result<Prescription> Create(
        Guid patientId,
        Guid doctorId,
        string medicationName,
        string dosage,
        string instructions,
        Guid? consultationId = null,
        DateTime? expiresAt = null)
    {
        if (patientId == Guid.Empty)
            return Result.Failure<Prescription>(Error.Validation(nameof(PatientId), "Patient is required."));

        if (doctorId == Guid.Empty)
            return Result.Failure<Prescription>(Error.Validation(nameof(DoctorId), "Doctor is required."));

        if (string.IsNullOrWhiteSpace(medicationName))
            return Result.Failure<Prescription>(Error.Validation(nameof(MedicationName), "Medication name cannot be empty."));

        if (string.IsNullOrWhiteSpace(dosage))
            return Result.Failure<Prescription>(Error.Validation(nameof(Dosage), "Dosage cannot be empty."));

        return Result.Success(new Prescription
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ConsultationId = consultationId,
            MedicationName = medicationName.Trim(),
            Dosage = dosage.Trim(),
            Instructions = instructions.Trim(),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        });
    }
}
