namespace MyProject.Infrastructure.Messaging.Sagas
{
    // Saga mesajları
    public record OrderSubmitted(Guid CorrelationId, Guid ProductId, int Quantity);
    public record OrderApproved(Guid CorrelationId);
    public record OrderRejected(Guid CorrelationId, string Reason);
    public record OrderCompleted(Guid CorrelationId);
}
