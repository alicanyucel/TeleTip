namespace MyProject.Application.Features.Products.Dtos
{
    public sealed record ProductDto(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int Stock,
        DateTime CreatedAt);
}
