using MassTransit;
using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Infrastructure.Messaging
{
    internal sealed class EventBus(IBus bus) : IEventBus
    {
        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
            => await bus.Publish(message, cancellationToken);
    }
}
