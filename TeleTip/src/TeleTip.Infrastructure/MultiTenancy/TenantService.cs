using Microsoft.AspNetCore.Http;
using MyProject.Application.Abstractions.MultiTenancy;

namespace MyProject.Infrastructure.MultiTenancy
{
    internal sealed class TenantService(IHttpContextAccessor httpContextAccessor) : ITenantService
    {
        public string? GetCurrentTenantId()
            => httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
    }
}
