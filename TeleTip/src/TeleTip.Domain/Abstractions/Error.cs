namespace MyProject.Domain.Abstractions
{
    public record Error(string Code, string Description)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "Null value was provided.");

        public static Error NotFound(string entity) => new($"{entity}.NotFound", $"{entity} was not found.");
        public static Error Validation(string field, string message) => new($"Validation.{field}", message);
        public static Error Conflict(string entity) => new($"{entity}.Conflict", $"{entity} already exists.");
        public static Error Unauthorized() => new("Auth.Unauthorized", "Unauthorized access.");
    }
}
