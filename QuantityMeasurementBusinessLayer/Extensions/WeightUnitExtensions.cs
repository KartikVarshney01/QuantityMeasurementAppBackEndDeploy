using QuantityMeasurementAppModelLayer.Enums;

namespace QuantityMeasurementAppBusinessLayer.Extensions;

/// <summary>Conversion helpers for <see cref="WeightUnit"/>. Base unit = Kilogram.</summary>
// This file helps us convert different weight units like Kilograms or Pounds.
public static class WeightUnitExtensions
{
    public static double GetConversionFactor(this WeightUnit unit) => unit switch
    {
        WeightUnit.Kilogram => 1.0,
        WeightUnit.Gram     => 0.001,
        WeightUnit.Pound    => 0.453592,
        _                   => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
    };

    public static double ToBaseUnit(this WeightUnit unit, double value)
        => value * unit.GetConversionFactor();

    public static double FromBaseUnit(this WeightUnit unit, double baseValue)
        => baseValue / unit.GetConversionFactor();

    public static string GetMeasurementType(this WeightUnit _) => "WEIGHT";
}
