using QuantityMeasurementAppBusinessLayer.Exceptions;
using QuantityMeasurementAppModelLayer.DTOs;

namespace QuantityMeasurementAppBusinessLayer.Engines;

/// <summary>
/// Stateless validation helpers used by the service layer.
/// Throws <see cref="QuantityMeasurementException"/> (mapped to HTTP 400) on failure.
/// </summary>
// This file checks if the data we received makes sense (like not comparing Feet to Grams).
public static class ValidationEngine
{
    /// <summary>Asserts that both quantities share the same measurement category.</summary>
    /// <exception cref="QuantityMeasurementException">When categories differ.</exception>
    public static void ValidateSameMeasurement(QuantityDTO q1, QuantityDTO q2)
    {
        if (!string.Equals(q1.Category, q2.Category, StringComparison.OrdinalIgnoreCase))
            throw new QuantityMeasurementException(
                $"Cannot operate across different categories: " +
                $"'{q1.Category}' and '{q2.Category}'.");
    }

    /// <summary>Asserts that neither <paramref name="q1"/> nor <paramref name="q2"/> is null.</summary>
    /// <exception cref="QuantityMeasurementException">When either argument is null.</exception>
    public static void ValidateNotNull(QuantityDTO? q1, QuantityDTO? q2 = null)
    {
        if (q1 is null)
            throw new QuantityMeasurementException("First quantity (q1) cannot be null.");
        if (q2 is null && q2 != null) // pattern kept symmetric
            throw new QuantityMeasurementException("Second quantity (q2) cannot be null.");
    }

    /// <summary>Asserts that <paramref name="targetUnit"/> is not null or whitespace.</summary>
    /// <exception cref="QuantityMeasurementException">When target unit is empty.</exception>
    public static void ValidateTargetUnit(string? targetUnit)
    {
        if (string.IsNullOrWhiteSpace(targetUnit))
            throw new QuantityMeasurementException("Target unit cannot be null or empty.");
    }
}
