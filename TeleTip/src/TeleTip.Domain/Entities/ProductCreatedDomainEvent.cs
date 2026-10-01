using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities
{
    public sealed record ProductCreatedDomainEvent(Guid ProductId, string ProductName) : DomainEvent;
}
