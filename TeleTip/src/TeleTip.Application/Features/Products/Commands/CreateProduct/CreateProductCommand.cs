using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Products.Commands.CreateProduct
{
    public sealed record CreateProductCommand(
        string Name,
        string Description,
        decimal Price,
        int Stock) : ICommand<Guid>;
}
