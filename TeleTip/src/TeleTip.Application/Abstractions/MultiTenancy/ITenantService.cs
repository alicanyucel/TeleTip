namespace MyProject.Application.Abstractions.MultiTenancy
{
    public interface ITenantService
    {
        string? GetCurrentTenantId();
    }
}
