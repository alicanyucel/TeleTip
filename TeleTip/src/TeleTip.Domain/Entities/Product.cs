using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities
{
    public sealed class Product : Entity
    {
        private Product() { }

        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public decimal Price { get; private set; }
        public int Stock { get; private set; }

        public static Result<Product> Create(string name, string description, decimal price, int stock)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<Product>(Error.Validation(nameof(Name), "Name cannot be empty."));

            if (price <= 0)
                return Result.Failure<Product>(Error.Validation(nameof(Price), "Price must be greater than zero."));

            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Stock = stock
            };

            product.RaiseDomainEvent(new ProductCreatedDomainEvent(product.Id, product.Name));

            return Result.Success(product);
        }

        public Result Update(string name, string description, decimal price, int stock)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(Error.Validation(nameof(Name), "Name cannot be empty."));

            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }
    }
}
