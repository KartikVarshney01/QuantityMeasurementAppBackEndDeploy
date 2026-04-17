using QuantityMeasurementAppModelLayer.Enums;

namespace QuantityMeasurementAppBusinessLayer.Extensions;

/// <summary>Conversion helpers for <see cref="LengthUnit"/>. Base unit = Feet.</summary>
// This file helps us change different length units (like Feet or Inches) into a "Base" unit.
public static class LengthUnitExtensions
{
    public static double GetConversionFactor(this LengthUnit unit) => unit switch
    {
        LengthUnit.Feet       => 1.0,
        LengthUnit.Inch       => 1.0 / 12.0,
        LengthUnit.Yard       => 3.0,
        LengthUnit.Centimeter => 1.0 / 30.48,
        _                     => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
    };

    /// <summary>Converts <paramref name="value"/> in this unit to Feet (base).</summary>
    public static double ToBaseUnit(this LengthUnit unit, double value)
        => value * unit.GetConversionFactor();

    /// <summary>Converts <paramref name="baseValue"/> in Feet back to this unit.</summary>
    public static double FromBaseUnit(this LengthUnit unit, double baseValue)
        => baseValue / unit.GetConversionFactor();

    public static string GetMeasurementType(this LengthUnit _) => "LENGTH";
}
