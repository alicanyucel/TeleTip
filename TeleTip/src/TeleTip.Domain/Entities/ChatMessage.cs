using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class ChatMessage : Entity
{
    private ChatMessage() { }

    public Guid ConsultationId { get; private set; }
    public string SenderId { get; private set; } = default!;
    public ConsultationParticipantRole SenderRole { get; private set; }
    public string Message { get; private set; } = default!;
    public DateTime SentAt { get; private set; }

    public static Result<ChatMessage> Create(
        Guid consultationId,
        string senderId,
        ConsultationParticipantRole senderRole,
        string message)
    {
        if (consultationId == Guid.Empty)
            return Result.Failure<ChatMessage>(Error.Validation(nameof(ConsultationId), "Consultation is required."));

        if (string.IsNullOrWhiteSpace(senderId))
            return Result.Failure<ChatMessage>(Error.Validation(nameof(SenderId), "Sender is required."));

        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure<ChatMessage>(Error.Validation(nameof(Message), "Message cannot be empty."));

        return Result.Success(new ChatMessage
        {
            ConsultationId = consultationId,
            SenderId = senderId,
            SenderRole = senderRole,
            Message = message.Trim(),
            SentAt = DateTime.UtcNow
        });
    }
}

public enum ConsultationParticipantRole
{
    Doctor,
    Patient
}
