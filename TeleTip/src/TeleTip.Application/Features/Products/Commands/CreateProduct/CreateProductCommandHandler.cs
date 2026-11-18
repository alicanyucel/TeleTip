using MyProject.Application.Abstractions.Cache;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Features.Products;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Products.Commands.CreateProduct
{
    internal sealed class CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService) : ICommandHandler<CreateProductCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var result = Product.Create(request.Name, request.Description, request.Price, request.Stock);

            if (result.IsFailure)
                return Result.Failure<Guid>(result.Error);

            await productRepository.AddAsync(result.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cacheService.RemoveAsync(ProductCacheKeys.All, cancellationToken);

            return Result.Success(result.Value.Id);
        }
    }
}
