using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class TelemedicineConsultation : Entity
{
    private TelemedicineConsultation() { }

    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public ConsultationStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public static Result<TelemedicineConsultation> Create(
        Guid patientId,
        Guid doctorId,
        DateTime scheduledAt)
    {
        if (patientId == Guid.Empty)
            return Result.Failure<TelemedicineConsultation>(Error.Validation(nameof(PatientId), "Patient is required."));

        if (doctorId == Guid.Empty)
            return Result.Failure<TelemedicineConsultation>(Error.Validation(nameof(DoctorId), "Doctor is required."));

        return Result.Success(new TelemedicineConsultation
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ScheduledAt = scheduledAt,
            Status = ConsultationStatus.Scheduled
        });
    }

    public void Start()
    {
        StartedAt = DateTime.UtcNow;
        Status = ConsultationStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(string? notes = null)
    {
        EndedAt = DateTime.UtcNow;
        Notes = notes;
        Status = ConsultationStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = ConsultationStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum ConsultationStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}
