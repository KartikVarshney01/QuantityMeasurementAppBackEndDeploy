using QuantityMeasurementAppModelLayer.Enums;

namespace QuantityMeasurementAppBusinessLayer.Extensions;

/// <summary>Conversion helpers for <see cref="VolumeUnit"/>. Base unit = Litre.</summary>
// We use Litre as our primary unit. 
// It's like the "Gold Standard" that everything else is measured against.
public static class VolumeUnitExtensions
{
    public static double GetConversionFactor(this VolumeUnit unit) => unit switch
    {
        VolumeUnit.Litre      => 1.0,
        VolumeUnit.Millilitre => 0.001,
        VolumeUnit.Gallon     => 3.78541,
        _                     => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
    };

    public static double ToBaseUnit(this VolumeUnit unit, double value)
        => value * unit.GetConversionFactor();

    public static double FromBaseUnit(this VolumeUnit unit, double baseValue)
        => baseValue / unit.GetConversionFactor();

    public static string GetMeasurementType(this VolumeUnit _) => "VOLUME";
}
