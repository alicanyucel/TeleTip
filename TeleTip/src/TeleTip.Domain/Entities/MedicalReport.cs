using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class MedicalReport : Entity
{
    private MedicalReport() { }

    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid? ConsultationId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public DateTime IssuedAt { get; private set; }

    public static Result<MedicalReport> Create(
        Guid patientId,
        Guid doctorId,
        string title,
        string content,
        Guid? consultationId = null)
    {
        if (patientId == Guid.Empty)
            return Result.Failure<MedicalReport>(Error.Validation(nameof(PatientId), "Patient is required."));

        if (doctorId == Guid.Empty)
            return Result.Failure<MedicalReport>(Error.Validation(nameof(DoctorId), "Doctor is required."));

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<MedicalReport>(Error.Validation(nameof(Title), "Title cannot be empty."));

        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<MedicalReport>(Error.Validation(nameof(Content), "Content cannot be empty."));

        return Result.Success(new MedicalReport
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ConsultationId = consultationId,
            Title = title.Trim(),
            Content = content.Trim(),
            IssuedAt = DateTime.UtcNow
        });
    }
}
