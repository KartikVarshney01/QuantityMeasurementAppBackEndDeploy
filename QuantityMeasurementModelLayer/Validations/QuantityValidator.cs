using System.ComponentModel.DataAnnotations;
using QuantityMeasurementAppModelLayer.DTOs;

namespace QuantityMeasurementAppModelLayer.Validations;

/// <summary>
/// Centralised validation helper for all quantity-related DTOs.
/// Used by the service layer and console app to validate input before processing.
/// The Web API layer uses DataAnnotation attributes for automatic model-binding
/// validation; this class provides the same checks programmatically for non-HTTP callers.
/// </summary>
// This file makes sure that the numbers and units the user gives us are valid.
public static class QuantityValidator
{
    // ── Valid category set ────────────────────────────────────────────

    private static readonly HashSet<string> ValidCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "LENGTH", "WEIGHT", "VOLUME", "TEMPERATURE"
    };

    // ── QuantityDTO validation ────────────────────────────────────────

    /// <summary>
    /// Validates a <see cref="QuantityDTO"/> using DataAnnotation rules
    /// and custom business rules.
    /// </summary>
    /// <param name="dto">The DTO to validate.</param>
    /// <param name="paramName">Parameter name for error messages (e.g. "q1").</param>
    /// <exception cref="ArgumentNullException">When <paramref name="dto"/> is null.</exception>
    /// <exception cref="ValidationException">When any validation rule fails.</exception>
    public static void Validate(QuantityDTO dto, string paramName = "quantity")
    {
        ArgumentNullException.ThrowIfNull(dto, paramName);

        // Run DataAnnotation validators
        var context = new ValidationContext(dto) { MemberName = paramName };
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(dto, context, results, validateAllProperties: true))
        {
            string errors = string.Join("; ", results.Select(r => r.ErrorMessage));
            throw new ValidationException($"Validation failed for {paramName}: {errors}");
        }

        // Custom: value must be a finite number
        if (double.IsNaN(dto.Value) || double.IsInfinity(dto.Value))
            throw new ValidationException($"{paramName}.Value must be a finite number.");

        // Custom: category must be one of the four known types
        if (!ValidCategories.Contains(dto.Category))
            throw new ValidationException(
                $"{paramName}.Category '{dto.Category}' is not valid. " +
                $"Must be one of: {string.Join(", ", ValidCategories)}.");

        // Custom: unit name must contain only letters, digits, and spaces
        if (!System.Text.RegularExpressions.Regex.IsMatch(dto.UnitName, @"^[a-zA-Z][a-zA-Z0-9\s]*$"))
            throw new ValidationException(
                $"{paramName}.UnitName '{dto.UnitName}' contains invalid characters.");
    }

    /// <summary>
    /// Validates two <see cref="QuantityDTO"/> instances and asserts they share the same category.
    /// </summary>
    public static void ValidatePair(QuantityDTO q1, QuantityDTO q2)
    {
        Validate(q1, "q1");
        Validate(q2, "q2");

        if (!string.Equals(q1.Category, q2.Category, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException(
                $"Category mismatch: q1 is '{q1.Category}' but q2 is '{q2.Category}'. " +
                "Both quantities must belong to the same measurement category.");
    }

    // ── QuantityRequest validation ────────────────────────────────────

    /// <summary>
    /// Validates a <see cref="QuantityRequest"/> using DataAnnotation rules.
    /// </summary>
    public static void Validate(QuantityRequest request, string paramName = "request")
    {
        ArgumentNullException.ThrowIfNull(request, paramName);

        var context = new ValidationContext(request) { MemberName = paramName };
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request, context, results, validateAllProperties: true))
        {
            string errors = string.Join("; ", results.Select(r => r.ErrorMessage));
            throw new ValidationException($"Validation failed for {paramName}: {errors}");
        }

        if (double.IsNaN(request.Value) || double.IsInfinity(request.Value))
            throw new ValidationException($"{paramName}.Value must be a finite number.");
    }

    /// <summary>
    /// Validates a <see cref="BinaryOperationRequest"/>.
    /// </summary>
    public static void Validate(BinaryOperationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        Validate(request.Q1, "request.Q1");
        Validate(request.Q2, "request.Q2");

        if (!string.Equals(request.Q1.Category, request.Q2.Category,
                           StringComparison.OrdinalIgnoreCase))
            throw new ValidationException(
                $"Q1.Category '{request.Q1.Category}' and Q2.Category '{request.Q2.Category}' must match.");
    }

    /// <summary>
    /// Validates a <see cref="ConversionRequest"/>.
    /// </summary>
    public static void Validate(ConversionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        Validate(request.Quantity, "request.Quantity");

        if (string.IsNullOrWhiteSpace(request.TargetUnit))
            throw new ValidationException("TargetUnit is required.");

        if (request.TargetUnit.Length > 50)
            throw new ValidationException("TargetUnit cannot exceed 50 characters.");
    }

    // ── Target unit validation ────────────────────────────────────────

    /// <summary>
    /// Asserts that <paramref name="targetUnit"/> is a non-empty string.
    /// </summary>
    public static void ValidateTargetUnit(string? targetUnit)
    {
        if (string.IsNullOrWhiteSpace(targetUnit))
            throw new ValidationException("TargetUnit cannot be null or empty.");

        if (targetUnit.Length > 50)
            throw new ValidationException("TargetUnit cannot exceed 50 characters.");
    }

    // ── Category guard ────────────────────────────────────────────────

    /// <summary>
    /// Returns true when the category string is one of the four known types.
    /// </summary>
    public static bool IsValidCategory(string? category)
        => !string.IsNullOrWhiteSpace(category) && ValidCategories.Contains(category);

    // ── Temperature arithmetic guard ──────────────────────────────────

    /// <summary>
    /// Throws <see cref="ValidationException"/> when the category is TEMPERATURE
    /// and the operation is arithmetic (Add / Subtract / Divide).
    /// </summary>
    public static void AssertArithmeticSupported(string category, string operation)
    {
        if (string.Equals(category, "TEMPERATURE", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException(
                $"Temperature does not support {operation}. " +
                "Only Compare and Convert are allowed for TEMPERATURE.");
    }
}
