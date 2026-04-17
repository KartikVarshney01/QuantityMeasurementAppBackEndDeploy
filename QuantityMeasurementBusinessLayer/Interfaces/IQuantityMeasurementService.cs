using QuantityMeasurementAppModelLayer.DTOs;
using QuantityMeasurementAppModelLayer.Entities;

namespace QuantityMeasurementAppBusinessLayer.Interfaces;

/// <summary>
/// UC18 service contract — extends UC17 with <c>userId</c> on every
/// mutation operation so each record is associated with its owner.
/// History/query methods are also filtered by <c>userId</c> so users
/// can only see their own records.
/// </summary>
public interface IQuantityMeasurementService
{
    // ── Core operations ───────────────────────────────────────────────

    /// <summary>Compares two quantities in base units. Returns true when equal.</summary>
    bool Compare(QuantityDTO q1, QuantityDTO q2, long userId);

    /// <summary>Converts q1 to <paramref name="targetUnit"/>. Returns the result DTO.</summary>
    QuantityDTO Convert(QuantityDTO q1, string targetUnit, long userId);

    /// <summary>Adds q1 and q2. Result is expressed in q1's unit.</summary>
    QuantityDTO Add(QuantityDTO q1, QuantityDTO q2, long userId);

    /// <summary>Subtracts q2 from q1. Result is expressed in q1's unit.</summary>
    QuantityDTO Subtract(QuantityDTO q1, QuantityDTO q2, long userId);

    /// <summary>Divides q1 by q2. Returns a dimensionless scalar ratio.</summary>
    double Divide(QuantityDTO q1, QuantityDTO q2, long userId);

    // ── History / query methods ───────────────────────────────────────

    /// <summary>Returns all records for <paramref name="userId"/>, newest first.</summary>
    List<QuantityMeasurementEntity> GetHistory(long userId);

    /// <summary>Returns records for <paramref name="userId"/> filtered by operation type.</summary>
    List<QuantityMeasurementEntity> GetByOperation(string operation, long userId);

    /// <summary>Returns records for <paramref name="userId"/> filtered by measurement category.</summary>
    List<QuantityMeasurementEntity> GetByMeasurementType(string measurementType, long userId);

    /// <summary>Returns the total number of records belonging to <paramref name="userId"/>.</summary>
    int GetCount(long userId);
}
