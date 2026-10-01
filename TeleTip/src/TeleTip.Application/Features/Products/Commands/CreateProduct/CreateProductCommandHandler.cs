using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Products.Commands.CreateProduct
{
    internal sealed class CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork) : ICommandHandler<CreateProductCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var result = Product.Create(request.Name, request.Description, request.Price, request.Stock);

            if (result.IsFailure)
                return Result.Failure<Guid>(result.Error);

            await productRepository.AddAsync(result.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(result.Value.Id);
        }
    }
}
