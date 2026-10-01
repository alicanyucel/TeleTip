using MassTransit;
using Microsoft.Extensions.Logging;

namespace MyProject.Infrastructure.Messaging.Sagas
{
    public sealed class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        private readonly ILogger<OrderStateMachine> _logger;

        public State Submitted { get; private set; } = default!;
        public State Approved { get; private set; } = default!;
        public State Rejected { get; private set; } = default!;
        public State Completed { get; private set; } = default!;

        public Event<OrderSubmitted> OrderSubmittedEvent { get; private set; } = default!;
        public Event<OrderApproved> OrderApprovedEvent { get; private set; } = default!;
        public Event<OrderRejected> OrderRejectedEvent { get; private set; } = default!;
        public Event<OrderCompleted> OrderCompletedEvent { get; private set; } = default!;

        public OrderStateMachine(ILogger<OrderStateMachine> logger)
        {
            _logger = logger;

            InstanceState(x => x.CurrentState);

            Event(() => OrderSubmittedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
            Event(() => OrderApprovedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
            Event(() => OrderRejectedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
            Event(() => OrderCompletedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));

            Initially(
                When(OrderSubmittedEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.ProductId = ctx.Message.ProductId;
                        ctx.Saga.Quantity = ctx.Message.Quantity;
                        ctx.Saga.CreatedAt = DateTime.UtcNow;
                        _logger.LogInformation("Order submitted: {CorrelationId}", ctx.Saga.CorrelationId);
                    })
                    .TransitionTo(Submitted));

            During(Submitted,
                When(OrderApprovedEvent)
                    .Then(ctx => _logger.LogInformation("Order approved: {CorrelationId}", ctx.Saga.CorrelationId))
                    .TransitionTo(Approved),
                When(OrderRejectedEvent)
                    .Then(ctx => _logger.LogWarning("Order rejected: {CorrelationId} - {Reason}", ctx.Saga.CorrelationId, ctx.Message.Reason))
                    .TransitionTo(Rejected));

            During(Approved,
                When(OrderCompletedEvent)
                    .Then(ctx => _logger.LogInformation("Order completed: {CorrelationId}", ctx.Saga.CorrelationId))
                    .TransitionTo(Completed)
                    .Finalize());

            SetCompletedWhenFinalized();
        }
    }
}
