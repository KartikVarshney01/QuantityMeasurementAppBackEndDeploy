using QuantityMeasurementAppModelLayer.Enums;

namespace QuantityMeasurementAppBusinessLayer.Extensions;

/// <summary>
/// Conversion helpers for <see cref="TemperatureUnit"/>.
/// Base unit = Celsius. Conversions are non-linear (no simple scale factor).
/// </summary>
// This file helps us convert different temperature units like Celsius or Fahrenheit.
public static class TemperatureUnitExtensions
{
    /// <summary>Converts <paramref name="value"/> in this unit to Celsius (base).</summary>
    public static double ToBaseUnit(this TemperatureUnit unit, double value) => unit switch
    {
        TemperatureUnit.Celsius    => value,
        TemperatureUnit.Fahrenheit => (value - 32.0) * 5.0 / 9.0,
        TemperatureUnit.Kelvin     => value - 273.15,
        _                          => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
    };

    /// <summary>Converts <paramref name="baseValue"/> in Celsius back to this unit.</summary>
    public static double FromBaseUnit(this TemperatureUnit unit, double baseValue) => unit switch
    {
        TemperatureUnit.Celsius    => baseValue,
        TemperatureUnit.Fahrenheit => (baseValue * 9.0 / 5.0) + 32.0,
        TemperatureUnit.Kelvin     => baseValue + 273.15,
        _                          => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
    };

    public static string GetMeasurementType(this TemperatureUnit _) => "TEMPERATURE";
}
