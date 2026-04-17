using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementAppModelLayer.DTOs;

/// <summary>
/// Data Transfer Object for a single physical quantity.
/// Carries DataAnnotation attributes so the ASP.NET Core model binder validates
/// incoming JSON automatically before any service logic executes.
/// </summary>
/// <remarks>
/// <b>UnitName</b> examples: Feet, Inch, Yard, Centimeter, Kilogram, Gram, Pound,
/// Litre, Millilitre, Gallon, Celsius, Fahrenheit, Kelvin.<br/>
/// <b>Category</b> must be one of: LENGTH, WEIGHT, VOLUME, TEMPERATURE.
/// </remarks>
// This is a simple "Container" or "Envelope" that holds information about a single measurement.
public class QuantityDTO
{
    /// <summary>Numeric magnitude of the quantity. May be negative (e.g. -5°C).</summary>
    [Required(ErrorMessage = "Value is required.")]
    public double Value { get; set; }

    /// <summary>
    /// Unit name string, e.g. "Feet", "Kilogram", "Celsius".
    /// Case-insensitive — the service layer normalises it.
    /// </summary>
    [Required(ErrorMessage = "UnitName is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "UnitName must be between 1 and 50 characters.")]
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// Measurement category: LENGTH, WEIGHT, VOLUME, or TEMPERATURE.
    /// The service layer upper-cases this before processing.
    /// </summary>
    [Required(ErrorMessage = "Category is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Category must be between 1 and 50 characters.")]
    [RegularExpression("^(LENGTH|WEIGHT|VOLUME|TEMPERATURE)$",
        ErrorMessage = "Category must be LENGTH, WEIGHT, VOLUME, or TEMPERATURE.")]
    public string Category { get; set; } = string.Empty;

    // ── Constructors ──────────────────────────────────────────────────

    /// <summary>Parameterless constructor required for JSON model binding.</summary>
    public QuantityDTO() { }

    /// <summary>Initialises a fully populated QuantityDTO.</summary>
    /// <exception cref="ArgumentException">Thrown when unitName or category is null/whitespace.</exception>
    public QuantityDTO(double value, string unitName, string category)
    {
        if (string.IsNullOrWhiteSpace(unitName))
            throw new ArgumentException("UnitName cannot be empty.", nameof(unitName));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be empty.", nameof(category));

        Value    = value;
        UnitName = unitName.Trim();
        Category = category.Trim().ToUpperInvariant();
    }

    public override string ToString() => $"QuantityDTO({Value}, {UnitName}, {Category})";
}
