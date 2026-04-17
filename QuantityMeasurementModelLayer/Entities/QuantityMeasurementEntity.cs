using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuantityMeasurementAppModelLayer.Models;
using QuantityMeasurementAppModelLayer.Enums;

namespace QuantityMeasurementAppModelLayer.Entities;

/// <summary>
/// EF Core entity that maps to the <c>QuantityMeasurements</c> table.
/// <para>
/// Operand1 and Operand2 are stored as JSON strings (nvarchar(max)) using EF Core
/// value converters configured in <c>ApplicationDbContext.OnModelCreating</c>.
/// Result is stored as a JSON string to accommodate both numeric scalars and
/// structured QuantityDTO objects (conversion / arithmetic outcomes).
/// </para>
/// <para>
/// UC18: <c>UserId</c> foreign key added — every record is owned by the user
/// who performed the operation. History queries are now filtered per-user.
/// </para>
/// </summary>
[Table("QuantityMeasurements")]
public class QuantityMeasurementEntity
{
    // ── Primary Key ───────────────────────────────────────────────────

    /// <summary>Auto-increment primary key.</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id")]
    public int Id { get; set; }

    // ── UC18: Owner ───────────────────────────────────────────────────

    /// <summary>
    /// Foreign key to the <c>Users</c> table.
    /// Identifies which authenticated user performed this operation.
    /// </summary>
    [Column("UserId")]
    public long UserId { get; set; }

    // ── Operation metadata ────────────────────────────────────────────

    /// <summary>
    /// Name of the operation performed (Compare / Convert / Add / Subtract / Divide).
    /// Stored as a plain nvarchar(50) string — the string value of <see cref="OperationType"/>.
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("Operation")]
    public string Operation { get; set; } = string.Empty;

    // ── Operands (JSON columns) ───────────────────────────────────────

    /// <summary>
    /// First operand serialised as JSON.
    /// Mapped by EF Core value converter in <c>ApplicationDbContext</c>.
    /// </summary>
    [Column("Operand1")]
    public QuantityModel<object>? Operand1 { get; set; }

    /// <summary>
    /// Second operand serialised as JSON. Null for unary operations (Convert).
    /// </summary>
    [Column("Operand2")]
    public QuantityModel<object>? Operand2 { get; set; }

    // ── Result (JSON column) ──────────────────────────────────────────

    /// <summary>
    /// Result value serialised as JSON. May be a boolean (Compare),
    /// a scalar double (Divide), or a QuantityDTO string (Convert / Add / Subtract).
    /// </summary>
    [Column("Result")]
    public object? Result { get; set; }

    // ── Error tracking ────────────────────────────────────────────────

    /// <summary>True when an exception was thrown during the operation.</summary>
    [Column("HasError")]
    public bool HasError { get; set; }

    /// <summary>Exception message when <see cref="HasError"/> is true; otherwise null.</summary>
    [MaxLength(500)]
    [Column("ErrorMessage")]
    public string? ErrorMessage { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────

    /// <summary>UTC timestamp set on insert.</summary>
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    // ── EF Core lifecycle hook ────────────────────────────────────────

    public void OnCreating() => CreatedAt = DateTime.UtcNow;

    // ── Constructors ──────────────────────────────────────────────────

    /// <summary>Parameterless constructor required by EF Core materialisation.</summary>
    public QuantityMeasurementEntity() { }

    /// <summary>Creates a successful single-operand record (e.g. Convert).</summary>
    public QuantityMeasurementEntity(
        long userId, string operation,
        QuantityModel<object> operand1, object result)
    {
        UserId    = userId;
        Operation = operation;
        Operand1  = operand1;
        Result    = result;
        HasError  = false;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Creates a successful two-operand record (e.g. Add, Compare).</summary>
    public QuantityMeasurementEntity(
        long userId, string operation,
        QuantityModel<object> operand1,
        QuantityModel<object> operand2, object result)
    {
        UserId    = userId;
        Operation = operation;
        Operand1  = operand1;
        Operand2  = operand2;
        Result    = result;
        HasError  = false;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Creates an error record.</summary>
    public QuantityMeasurementEntity(long userId, string operation, string errorMessage)
    {
        UserId       = userId;
        Operation    = operation;
        HasError     = true;
        ErrorMessage = errorMessage;
        CreatedAt    = DateTime.UtcNow;
    }

    // ── Display ───────────────────────────────────────────────────────

    public override string ToString()
    {
        string time = CreatedAt.ToString("HH:mm:ss");

        if (HasError)
            return $"[{time}] {Operation} => ERROR: {ErrorMessage}";

        string ops = Operand2 != null
            ? $"{Operand1} | {Operand2}"
            : $"{Operand1}";

        return $"[{time}] {Operation} | {ops} => {Result}";
    }
}
