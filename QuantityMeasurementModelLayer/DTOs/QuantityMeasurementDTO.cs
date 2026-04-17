using System.ComponentModel.DataAnnotations;
using QuantityMeasurementAppModelLayer.Models;
using QuantityMeasurementAppModelLayer.Entities;

namespace QuantityMeasurementAppModelLayer.DTOs;

/// <summary>
/// API-layer DTO that mirrors <see cref="QuantityMeasurementEntity"/> for history responses.
/// Static factory methods provide clean conversion between entity and DTO representations.
/// </summary>
public class QuantityMeasurementDTO
{
    /// <summary>Database-generated primary key.</summary>
    public int Id { get; set; }

    /// <summary>Operation type string (Compare / Convert / Add / Subtract / Divide).</summary>
    [Required]
    [StringLength(50)]
    public string Operation { get; set; } = string.Empty;

    /// <summary>First operand model. Null for error records.</summary>
    public object? Operand1 { get; set; }

    /// <summary>Second operand model. Null for unary operations or error records.</summary>
    public object? Operand2 { get; set; }

    /// <summary>Operation result. Null for error records.</summary>
    public object? Result { get; set; }

    /// <summary>True when the operation ended with an exception.</summary>
    public bool HasError { get; set; }

    /// <summary>Exception message for error records; null otherwise.</summary>
    [StringLength(500)]
    public string? ErrorMessage { get; set; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAt { get; set; }

    // ── Parameterless ctor ────────────────────────────────────────────

    public QuantityMeasurementDTO() { }

    // ── Static factory: Entity → DTO ─────────────────────────────────

    /// <summary>Converts a single <see cref="QuantityMeasurementEntity"/> to its DTO form.</summary>
    public static QuantityMeasurementDTO FromEntity(QuantityMeasurementEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new QuantityMeasurementDTO
        {
            Id           = entity.Id,
            Operation    = entity.Operation,
            Operand1     = entity.Operand1,
            Operand2     = entity.Operand2,
            Result       = entity.Result,
            HasError     = entity.HasError,
            ErrorMessage = entity.ErrorMessage,
            CreatedAt    = entity.CreatedAt
        };
    }

    // ── Static factory: List<Entity> → List<DTO> ─────────────────────

    public static List<QuantityMeasurementDTO> FromEntityList(
        List<QuantityMeasurementEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return entities.Select(FromEntity).ToList();
    }

    // ── Instance: DTO → Entity ────────────────────────────────────────

    public QuantityMeasurementEntity ToEntity() =>
        new QuantityMeasurementEntity
        {
            Id           = Id,
            Operation    = Operation,
            Operand1     = Operand1 as QuantityModel<object>,
            Operand2     = Operand2 as QuantityModel<object>,
            Result       = Result,
            HasError     = HasError,
            ErrorMessage = ErrorMessage,
            CreatedAt    = CreatedAt
        };

    // ── Static factory: List<DTO> → List<Entity> ─────────────────────

    public static List<QuantityMeasurementEntity> ToEntityList(
        List<QuantityMeasurementDTO> dtos)
    {
        ArgumentNullException.ThrowIfNull(dtos);
        return dtos.Select(d => d.ToEntity()).ToList();
    }

    public override string ToString() =>
        $"[{CreatedAt:HH:mm:ss}] {Operation} => " +
        $"{(HasError ? "ERROR: " + ErrorMessage : Result?.ToString())}";
}
