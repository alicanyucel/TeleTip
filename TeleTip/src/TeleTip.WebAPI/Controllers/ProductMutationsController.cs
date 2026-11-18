using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Application.Abstractions.Cache;
using MyProject.Application.Features.Products;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.Persistence;

namespace MyProject.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductMutationsController(AppDbContext db, ICacheService cache) : ControllerBase
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (product is null)
            return NotFound();

        var result = product.Update(request.Name, request.Description, request.Price, request.Stock);
        if (result.IsFailure)
            return BadRequest(result.Error);

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(ProductCacheKeys.All, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (product is null)
            return NotFound();

        product.SoftDelete();
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(ProductCacheKeys.All, ct);
        return NoContent();
    }

    public sealed record UpdateProductRequest(string Name, string Description, decimal Price, int Stock);
}
