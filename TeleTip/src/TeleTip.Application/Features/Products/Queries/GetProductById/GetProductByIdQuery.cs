using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Features.Products.Dtos;

namespace MyProject.Application.Features.Products.Queries.GetProductById
{
    public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;
}
