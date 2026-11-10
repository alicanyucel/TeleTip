using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MyProject.WebAPI.Contracts;

namespace MyProject.WebAPI.Hubs;

[Authorize]
public sealed class ConsultationHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, Participant>> Rooms = new();

    public async Task JoinConsultation(JoinConsultationRequest request)
    {
        if (request.ConsultationId == Guid.Empty)
            throw new HubException("Consultation id is required.");

        var room = Rooms.GetOrAdd(request.ConsultationId, _ => new ConcurrentDictionary<string, Participant>());
        var connectionId = Context.ConnectionId;
        var participant = new Participant(connectionId, request.Role);

        if (!room.TryAdd(connectionId, participant))
            throw new HubException("Connection is already in this consultation.");

        if (room.Values.Count(p => p.Role == request.Role) > 1)
        {
            room.TryRemove(connectionId, out _);
            throw new HubException($"A {request.Role} is already connected to this consultation.");
        }

        await Groups.AddToGroupAsync(connectionId, GroupName(request.ConsultationId));
        await Clients.Caller.SendAsync("ConsultationJoined", new
        {
            request.ConsultationId,
            Role = request.Role.ToString(),
            ConnectionId = connectionId,
            ParticipantCount = room.Count
        });

        await Clients.OthersInGroup(request.ConsultationId.ToString("N"))
            .SendAsync("ParticipantJoined", request.Role.ToString(), connectionId);
    }

    public async Task SendChatMessage(Guid consultationId, string message)
    {
        EnsureJoined(consultationId);

        if (string.IsNullOrWhiteSpace(message))
            throw new HubException("Message cannot be empty.");

        var participant = Rooms[consultationId][Context.ConnectionId];
        var chatMessage = new ChatMessage(
            consultationId,
            Context.UserIdentifier ?? Context.ConnectionId,
            participant.Role,
            message.Trim(),
            DateTime.UtcNow);

        await Clients.Group(GroupName(consultationId)).SendAsync("ChatMessageReceived", chatMessage);
    }

    public async Task SendOffer(Guid consultationId, string sdp)
    {
        EnsureJoined(consultationId);
        await Clients.OthersInGroup(GroupName(consultationId)).SendAsync(
            "WebRtcOfferReceived",
            new WebRtcOffer(consultationId, Context.ConnectionId, sdp));
    }

    public async Task SendAnswer(Guid consultationId, string sdp)
    {
        EnsureJoined(consultationId);
        await Clients.OthersInGroup(GroupName(consultationId)).SendAsync(
            "WebRtcAnswerReceived",
            new WebRtcAnswer(consultationId, Context.ConnectionId, sdp));
    }

    public async Task SendIceCandidate(
        Guid consultationId,
        string candidate,
        string? sdpMid,
        int? sdpMLineIndex)
    {
        EnsureJoined(consultationId);
        await Clients.OthersInGroup(GroupName(consultationId)).SendAsync(
            "IceCandidateReceived",
            new IceCandidate(
                consultationId,
                Context.ConnectionId,
                candidate,
                sdpMid,
                sdpMLineIndex));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var roomEntry in Rooms)
        {
            if (!roomEntry.Value.TryRemove(Context.ConnectionId, out var participant))
                continue;

            await Clients.Group(GroupName(roomEntry.Key)).SendAsync(
                "ParticipantLeft",
                participant.Role.ToString(),
                Context.ConnectionId);

            if (roomEntry.Value.IsEmpty)
                Rooms.TryRemove(roomEntry.Key, out _);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private void EnsureJoined(Guid consultationId)
    {
        if (!Rooms.TryGetValue(consultationId, out var room) ||
            !room.ContainsKey(Context.ConnectionId))
        {
            throw new HubException("Join the consultation before sending messages.");
        }
    }

    private static string GroupName(Guid consultationId) => consultationId.ToString("N");

    private sealed record Participant(string ConnectionId, ConsultationParticipantRole Role);
}
