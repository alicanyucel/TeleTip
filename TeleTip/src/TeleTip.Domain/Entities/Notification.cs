using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class Notification : Entity
{
    private Notification() { }

    public Guid PatientId { get; private set; }
    public Guid? DoctorId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public static Result<Notification> Create(
        Guid patientId,
        NotificationType type,
        string title,
        string message,
        Guid? doctorId = null)
    {
        if (patientId == Guid.Empty)
            return Result.Failure<Notification>(Error.Validation(nameof(PatientId), "Patient is required."));

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Notification>(Error.Validation(nameof(Title), "Title cannot be empty."));

        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure<Notification>(Error.Validation(nameof(Message), "Message cannot be empty."));

        return Result.Success(new Notification
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim()
        });
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum NotificationType
{
    AiAlert,
    ConsultationReminder,
    PrescriptionIssued,
    ReportIssued,
    General
}
