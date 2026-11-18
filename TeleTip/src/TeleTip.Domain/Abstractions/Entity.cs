namespace MyProject.Domain.Abstractions
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; }
        public string? TenantId { get; protected set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents() => _domainEvents.Clear();

        public void SetTenant(string tenantId) => TenantId = tenantId;

        public void SoftDelete()
        {
            if (IsDeleted)
                return;

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            MarkUpdated();
        }

        protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
    }
}
