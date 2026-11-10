namespace MyProject.WebAPI.Contracts;

public enum ConsultationParticipantRole
{
    Doctor,
    Patient
}

public sealed record JoinConsultationRequest(
    Guid ConsultationId,
    ConsultationParticipantRole Role);

public sealed record ChatMessage(
    Guid ConsultationId,
    string SenderId,
    ConsultationParticipantRole SenderRole,
    string Message,
    DateTime SentAt);

public sealed record WebRtcOffer(
    Guid ConsultationId,
    string SenderId,
    string Sdp);

public sealed record WebRtcAnswer(
    Guid ConsultationId,
    string SenderId,
    string Sdp);

public sealed record IceCandidate(
    Guid ConsultationId,
    string SenderId,
    string Candidate,
    string? SdpMid,
    int? SdpMLineIndex);
