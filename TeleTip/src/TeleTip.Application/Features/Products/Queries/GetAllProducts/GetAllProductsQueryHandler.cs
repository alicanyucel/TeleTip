using AutoMapper;
using MyProject.Application.Abstractions.Cache;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Features.Products.Dtos;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Products.Queries.GetAllProducts
{
    internal sealed class GetAllProductsQueryHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        IMapper mapper) : IQueryHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        private const string CacheKey = "products:all";

        public async Task<Result<IEnumerable<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var cached = await cacheService.GetAsync<IEnumerable<ProductDto>>(CacheKey, cancellationToken);
            if (cached is not null)
                return Result.Success(cached);

            var products = await productRepository.GetAllAsync(cancellationToken);
            var dtos = mapper.Map<IEnumerable<ProductDto>>(products);

            await cacheService.SetAsync(CacheKey, dtos, TimeSpan.FromMinutes(5), cancellationToken);

            return Result.Success(dtos);
        }
    }
}
