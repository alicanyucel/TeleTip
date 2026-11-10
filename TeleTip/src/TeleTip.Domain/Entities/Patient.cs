using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class Patient : Entity
{
    private Patient() { }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public DateOnly DateOfBirth { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }

    public static Result<Patient> Create(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string? phoneNumber = null,
        string? email = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<Patient>(Error.Validation(nameof(FirstName), "First name cannot be empty."));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<Patient>(Error.Validation(nameof(LastName), "Last name cannot be empty."));

        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Failure<Patient>(Error.Validation(nameof(DateOfBirth), "Date of birth cannot be in the future."));

        return Result.Success(new Patient
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            DateOfBirth = dateOfBirth,
            PhoneNumber = phoneNumber,
            Email = email
        });
    }
}
