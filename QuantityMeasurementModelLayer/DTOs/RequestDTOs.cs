using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementAppModelLayer.DTOs;

// ─────────────────────────────────────────────────────────────────────────────
// UC17: Request DTOs live in ModelLayer so they are shared between the Web API
// and any other consumer (console app, tests) without taking a project dependency
// on the API layer.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Represents a single physical quantity in an API request.
/// </summary>
// This is like an "Order Form" that the user fills out when they want to make a measurement.
public class QuantityRequest
{
    /// <summary>
    /// Numeric value of the quantity. Negative values are permitted (e.g. -5°C).
    /// </summary>
    [Required(ErrorMessage = "Value is required.")]
    public double Value { get; set; }

    /// <summary>
    /// Unit name, e.g. "Feet", "Kilogram", "Celsius".
    /// Case-insensitive — the service layer normalises casing.
    /// </summary>
    [Required(ErrorMessage = "UnitName is required.")]
    [StringLength(50, MinimumLength = 1,
        ErrorMessage = "UnitName must be between 1 and 50 characters.")]
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// Measurement category: LENGTH, WEIGHT, VOLUME, or TEMPERATURE.
    /// </summary>
    [Required(ErrorMessage = "Category is required.")]
    [StringLength(50, MinimumLength = 1,
        ErrorMessage = "Category must be between 1 and 50 characters.")]
    [RegularExpression("^(LENGTH|WEIGHT|VOLUME|TEMPERATURE)$",
        ErrorMessage = "Category must be LENGTH, WEIGHT, VOLUME, or TEMPERATURE.")]
    public string Category { get; set; } = string.Empty;

    public override string ToString() => $"QuantityRequest({Value}, {UnitName}, {Category})";
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Wraps two quantities for binary operations: Compare, Add, Subtract, Divide.
/// </summary>
public class BinaryOperationRequest
{
    /// <summary>First quantity (minuend / dividend / left operand).</summary>
    [Required(ErrorMessage = "First quantity (Q1) is required.")]
    public QuantityRequest Q1 { get; set; } = null!;

    /// <summary>Second quantity (subtrahend / divisor / right operand).</summary>
    [Required(ErrorMessage = "Second quantity (Q2) is required.")]
    public QuantityRequest Q2 { get; set; } = null!;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Wraps a source quantity and the target unit name for unit conversion.
/// </summary>
public class ConversionRequest
{
    /// <summary>The quantity to convert.</summary>
    [Required(ErrorMessage = "Quantity is required.")]
    public QuantityRequest Quantity { get; set; } = null!;

    /// <summary>
    /// Target unit name, e.g. "Inch", "Gram", "Fahrenheit".
    /// Must belong to the same category as <see cref="Quantity"/>.
    /// </summary>
    [Required(ErrorMessage = "TargetUnit is required.")]
    [StringLength(50, MinimumLength = 1,
        ErrorMessage = "TargetUnit must be between 1 and 50 characters.")]
    public string TargetUnit { get; set; } = string.Empty;
}
