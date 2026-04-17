using QuantityMeasurementAppBusinessLayer.Exceptions;
using QuantityMeasurementAppBusinessLayer.Extensions;
using QuantityMeasurementAppModelLayer.DTOs;
using QuantityMeasurementAppModelLayer.Enums;

namespace QuantityMeasurementAppBusinessLayer.Engines;

/// <summary>
/// Stateless conversion helpers used by <c>QuantityMeasurementServiceImpl</c>.
/// Delegates to the unit-specific extension methods for all numeric conversions.
/// </summary>
public static class ConversionEngine
{
    // ── Convert DTO value → base unit ────────────────────────────────
    // We first convert everything to a "Standard" or "Base" unit.
    // Think of it like a middle-ground so we can compare different things easily.

    /// <summary>Converts a <see cref="QuantityDTO"/> value to its category's base unit.</summary>
    /// <exception cref="QuantityMeasurementException">Unknown category or unit name.</exception>
    public static double ConvertToBase(QuantityDTO dto)
    {
        return dto.Category.ToUpperInvariant() switch
        {
            "LENGTH"      => ParseLength(dto.UnitName).ToBaseUnit(dto.Value),
            "WEIGHT"      => ParseWeight(dto.UnitName).ToBaseUnit(dto.Value),
            "VOLUME"      => ParseVolume(dto.UnitName).ToBaseUnit(dto.Value),
            "TEMPERATURE" => ParseTemperature(dto.UnitName).ToBaseUnit(dto.Value),
            _             => throw new QuantityMeasurementException(
                                 $"Unknown category: '{dto.Category}'.")
        };
    }

    // ── Convert base value → target unit ─────────────────────────────
    // After we have the value in the "Base" unit, we can turn it into any other unit.

    /// <summary>Converts a base-unit value back to the named target unit.</summary>
    /// <exception cref="QuantityMeasurementException">Unknown category or unit name.</exception>
    public static double ConvertFromBase(string category, string unitName, double baseValue)
    {
        return category.ToUpperInvariant() switch
        {
            "LENGTH"      => ParseLength(unitName).FromBaseUnit(baseValue),
            "WEIGHT"      => ParseWeight(unitName).FromBaseUnit(baseValue),
            "VOLUME"      => ParseVolume(unitName).FromBaseUnit(baseValue),
            "TEMPERATURE" => ParseTemperature(unitName).FromBaseUnit(baseValue),
            _             => throw new QuantityMeasurementException(
                                 $"Unknown category: '{category}'.")
        };
    }

    // ── Unit name parsers (alias-aware) ───────────────────────────────

    /// <summary>Parses a length unit name (case-insensitive, supports common aliases).</summary>
    public static LengthUnit ParseLength(string name) =>
        name.Trim().ToUpperInvariant() switch
        {
            "FEET" or "FOOT" or "FT"              => LengthUnit.Feet,
            "INCHES" or "INCH" or "IN"             => LengthUnit.Inch,
            "YARDS" or "YARD" or "YD"              => LengthUnit.Yard,
            "CENTIMETERS" or "CENTIMETER" or "CM"  => LengthUnit.Centimeter,
            _ => throw new QuantityMeasurementException($"Unknown length unit: '{name}'.")
        };

    /// <summary>Parses a weight unit name (case-insensitive, supports common aliases).</summary>
    public static WeightUnit ParseWeight(string name) =>
        name.Trim().ToUpperInvariant() switch
        {
            "KILOGRAM" or "KILOGRAMS" or "KG" => WeightUnit.Kilogram,
            "GRAM" or "GRAMS" or "G"          => WeightUnit.Gram,
            "POUND" or "POUNDS" or "LB"       => WeightUnit.Pound,
            _ => throw new QuantityMeasurementException($"Unknown weight unit: '{name}'.")
        };

    /// <summary>Parses a volume unit name (case-insensitive, supports common aliases).</summary>
    public static VolumeUnit ParseVolume(string name) =>
        name.Trim().ToUpperInvariant() switch
        {
            "LITRE" or "LITER" or "L"            => VolumeUnit.Litre,
            "MILLILITRE" or "MILLILITER" or "ML"  => VolumeUnit.Millilitre,
            "GALLON" or "GAL"                     => VolumeUnit.Gallon,
            _ => throw new QuantityMeasurementException($"Unknown volume unit: '{name}'.")
        };

    /// <summary>Parses a temperature unit name (case-insensitive, supports common aliases).</summary>
    public static TemperatureUnit ParseTemperature(string name) =>
        name.Trim().ToUpperInvariant() switch
        {
            "CELSIUS" or "C"    => TemperatureUnit.Celsius,
            "FAHRENHEIT" or "F" => TemperatureUnit.Fahrenheit,
            "KELVIN" or "K"     => TemperatureUnit.Kelvin,
            _ => throw new QuantityMeasurementException($"Unknown temperature unit: '{name}'.")
        };
}
