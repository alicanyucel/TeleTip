using MassTransit;

namespace MyProject.Infrastructure.Messaging.Sagas
{
    // Saga State Machine - Sipariş İşleme Örneği
    public sealed class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = default!;
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
