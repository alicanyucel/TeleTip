using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class Doctor : Entity
{
    private Doctor() { }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Specialization { get; private set; } = default!;
    public string LicenseNumber { get; private set; } = default!;
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }

    public static Result<Doctor> Create(
        string firstName,
        string lastName,
        string specialization,
        string licenseNumber,
        string? phoneNumber = null,
        string? email = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<Doctor>(Error.Validation(nameof(FirstName), "First name cannot be empty."));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<Doctor>(Error.Validation(nameof(LastName), "Last name cannot be empty."));

        if (string.IsNullOrWhiteSpace(specialization))
            return Result.Failure<Doctor>(Error.Validation(nameof(Specialization), "Specialization cannot be empty."));

        if (string.IsNullOrWhiteSpace(licenseNumber))
            return Result.Failure<Doctor>(Error.Validation(nameof(LicenseNumber), "License number cannot be empty."));

        return Result.Success(new Doctor
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Specialization = specialization.Trim(),
            LicenseNumber = licenseNumber.Trim(),
            PhoneNumber = phoneNumber,
            Email = email
        });
    }
}
