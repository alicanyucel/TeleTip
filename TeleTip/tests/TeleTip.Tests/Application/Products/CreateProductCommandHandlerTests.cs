using FluentAssertions;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Features.Products.Commands.CreateProduct;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;
using NSubstitute;

namespace MyProject.Tests.Application.Products
{
    public sealed class CreateProductCommandHandlerTests
    {
        private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly CreateProductCommandHandler _handler;

        public CreateProductCommandHandlerTests()
            => _handler = new CreateProductCommandHandler(_productRepository, _unitOfWork);

        [Fact]
        public async Task Handle_ValidCommand_ShouldReturnSuccessWithGuid()
        {
            // Arrange
            var command = new CreateProductCommand("Test Product", "Description", 99.99m, 10);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
            await _productRepository.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_EmptyName_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateProductCommand("", "Description", 99.99m, 10);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().StartWith("Validation");
        }

        [Fact]
        public async Task Handle_NegativePrice_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateProductCommand("Product", "Description", -1m, 10);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}
