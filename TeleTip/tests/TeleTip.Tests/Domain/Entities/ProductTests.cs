using FluentAssertions;
using MyProject.Domain.Entities;

namespace MyProject.Tests.Domain.Entities
{
    public sealed class ProductTests
    {
        [Fact]
        public void Create_ValidParameters_ShouldSucceed()
        {
            var result = Product.Create("Laptop", "Gaming laptop", 1500m, 5);

            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("Laptop");
            result.Value.Price.Should().Be(1500m);
            result.Value.DomainEvents.Should().HaveCount(1);
            result.Value.DomainEvents[0].Should().BeOfType<ProductCreatedDomainEvent>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_InvalidName_ShouldFail(string? name)
        {
            var result = Product.Create(name!, "Description", 100m, 1);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().StartWith("Validation");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Create_InvalidPrice_ShouldFail(decimal price)
        {
            var result = Product.Create("Product", "Description", price, 1);

            result.IsFailure.Should().BeTrue();
        }
    }
}
