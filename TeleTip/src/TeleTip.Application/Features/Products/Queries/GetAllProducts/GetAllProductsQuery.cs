using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Features.Products.Dtos;

namespace MyProject.Application.Features.Products.Queries.GetAllProducts
{
    public sealed record GetAllProductsQuery : IQuery<IEnumerable<ProductDto>>;
}
