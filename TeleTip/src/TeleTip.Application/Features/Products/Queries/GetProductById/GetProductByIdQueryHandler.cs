using AutoMapper;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Features.Products.Dtos;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Products.Queries.GetProductById
{
    internal sealed class GetProductByIdQueryHandler(
        IProductRepository productRepository,
        IMapper mapper) : IQueryHandler<GetProductByIdQuery, ProductDto>
    {
        public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

            if (product is null)
                return Result.Failure<ProductDto>(Error.NotFound(nameof(product)));

            return Result.Success(mapper.Map<ProductDto>(product));
        }
    }
}
