namespace QuantityMeasurementAppModelLayer.Models;

/// <summary>
/// Lightweight value-holder for one operand: its numeric value and unit.
/// Stored as a JSON column inside <c>QuantityMeasurementEntity</c>.
/// <para>
/// UC18: Reverted back.
/// </para>
/// </summary>
public class QuantityModel<T>
{
    /// <summary>Numeric magnitude of the quantity.</summary>
    public double Value { get; set; }

    /// <summary>Unit enum value (e.g. LengthUnit.Feet) or unit name string.</summary>
    public T Unit { get; set; } = default!;

    /// <summary>Parameterless constructor required for JSON deserialisation.</summary>
    public QuantityModel() { }

    /// <summary>Initialises a new quantity model with the given value and unit.</summary>
    public QuantityModel(double value, T unit)
    {
        Value = value;
        Unit  = unit;
    }

    public override string ToString() => $"{Value} {Unit}";
}
