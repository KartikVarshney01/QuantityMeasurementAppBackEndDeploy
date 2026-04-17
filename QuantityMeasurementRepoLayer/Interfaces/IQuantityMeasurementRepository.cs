using QuantityMeasurementAppModelLayer.Entities;

namespace QuantityMeasurementAppRepoLayer.Interfaces;

/// <summary>
/// UC18 repository contract — extends UC17 by scoping all query methods to a
/// specific <c>userId</c> so users can only retrieve their own records.
/// Satisfied by <c>EFCoreQuantityMeasurementRepository</c> (SQL Server) and
/// <c>QuantityMeasurementCacheRepository</c> (in-memory fallback).
/// </summary>
public interface IQuantityMeasurementRepository
{
    // ── Core CRUD ─────────────────────────────────────────────────────
    void Save(QuantityMeasurementEntity entity);
    void DeleteAll();

    // ── Filtered queries (scoped to userId) ───────────────────────────
    List<QuantityMeasurementEntity> GetAll(long userId);
    List<QuantityMeasurementEntity> GetByOperation(string operation, long userId);
    List<QuantityMeasurementEntity> GetByMeasurementType(string measurementType, long userId);

    // ── Aggregates (scoped to userId) ─────────────────────────────────
    int GetTotalCount(long userId);

    // ── Lifecycle ─────────────────────────────────────────────────────
    void CloseResources();
}
