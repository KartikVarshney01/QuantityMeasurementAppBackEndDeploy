using QuantityMeasurementAppBusinessLayer.Exceptions;

namespace QuantityMeasurementAppBusinessLayer.Engines;

/// <summary>
/// Stateless arithmetic helpers used by <c>QuantityMeasurementServiceImpl</c>.
/// All methods operate on pre-converted base-unit values.
/// Temperature operations are blocked here so the guard is centralised
/// and independently testable.
/// </summary>
public static class ArithmeticEngine
{
    private const double Epsilon = 1e-9;
    // We don't allow adding or subtracting temperature because it 
    // doesn't really make sense (like adding 20°C to 30°C doesn't make 50°C).

    /// <summary>Adds two base-unit values. Temperature is not supported.</summary>
    /// <exception cref="QuantityMeasurementException">When category is TEMPERATURE.</exception>
    public static double Add(double v1, double v2, string category)
    {
        BlockTemperature(category, "addition");
        return v1 + v2;
    }

    /// <summary>Subtracts <paramref name="v2"/> from <paramref name="v1"/>. Temperature not supported.</summary>
    /// <exception cref="QuantityMeasurementException">When category is TEMPERATURE.</exception>
    public static double Subtract(double v1, double v2, string category)
    {
        BlockTemperature(category, "subtraction");
        return v1 - v2;
    }

    /// <summary>
    /// Divides <paramref name="v1"/> by <paramref name="v2"/>.
    /// Returns a dimensionless scalar. Temperature not supported.
    /// </summary>
    /// <exception cref="QuantityMeasurementException">When category is TEMPERATURE or divisor is zero.</exception>
    public static double Divide(double v1, double v2, string category)
    {
        BlockTemperature(category, "division");

        if (Math.Abs(v2) < Epsilon)
            throw new QuantityMeasurementException("Division by zero is not allowed.");

        // After all the checks, we just do the normal math here.
        return v1 / v2;
    }

    // ── Private ───────────────────────────────────────────────────────

    private static void BlockTemperature(string category, string operation)
    {
        if (string.Equals(category, "TEMPERATURE", StringComparison.OrdinalIgnoreCase))
            throw new QuantityMeasurementException(
                $"Temperature {operation} is not supported. " +
                "Only Compare and Convert are allowed for TEMPERATURE.");
    }
}
