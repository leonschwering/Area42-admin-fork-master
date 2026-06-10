using Area42_1.Web.Models;
using System.Text.RegularExpressions;

namespace Area42_1.Web.Services;

public class AccountValidationService
{
    public ValidationResult ValidateName(string firstName, string lastName, string? tussenvoegsel = null)
    {
        // Names must contain only letters (A-Z, a-z)
        // Each part should be at least 2 characters and max 50 characters

        if (string.IsNullOrWhiteSpace(firstName))
            return ValidationResult.Failure("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            return ValidationResult.Failure("Last name is required");

        // Check first name: Aa-Zz format (letters only)
        if (!IsValidNamePart(firstName))
            return ValidationResult.Failure("First name must contain only letters (A-Z, a-z) and be between 2-50 characters");

        // Check last name: Aa-Zz format
        if (!IsValidNamePart(lastName))
            return ValidationResult.Failure("Last name must contain only letters (A-Z, a-z) and be between 2-50 characters");

        // Check tussenvoegsel if provided: Aa-Zz format
        if (!string.IsNullOrWhiteSpace(tussenvoegsel) && !IsValidNamePart(tussenvoegsel))
            return ValidationResult.Failure("Middle name must contain only letters (A-Z, a-z) and be between 2-50 characters");

        return ValidationResult.Success();
    }

    public ValidationResult ValidateAge(DateTime dateOfBirth)
    {
        var age = CalculateAge(dateOfBirth);

        if (age < 18)
            return ValidationResult.Failure("You must be at least 18 years old to register");

        if (age > 150) // Sanity check
            return ValidationResult.Failure("Please enter a valid date of birth");

        return ValidationResult.Success();
    }

    public int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;

        // Subtract 1 if birthday hasn't occurred this year
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;

        return age;
    }

    private bool IsValidNamePart(string namePart)
    {
        // Must be 2-50 characters, letters only (a-z, A-Z), spaces allowed within but not at start/end
        if (string.IsNullOrWhiteSpace(namePart))
            return false;

        // Trim and check length
        namePart = namePart.Trim();
        if (namePart.Length < 2 || namePart.Length > 50)
            return false;

        // Only letters and hyphens/apostrophes allowed (common in names)
        // But for Aa-Zz format, we'll be strict and allow letters and hyphens only
        return Regex.IsMatch(namePart, @"^[a-zA-Z\-']+$");
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string Message { get; private set; } = string.Empty;

    private ValidationResult(bool isValid, string message = "")
    {
        IsValid = isValid;
        Message = message;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string message) => new(false, message);
}
