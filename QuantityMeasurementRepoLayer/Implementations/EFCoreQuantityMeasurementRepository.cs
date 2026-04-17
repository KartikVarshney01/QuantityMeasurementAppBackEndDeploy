using Microsoft.EntityFrameworkCore;
using QuantityMeasurementAppModelLayer.Entities;
using QuantityMeasurementAppRepoLayer.Data;
using QuantityMeasurementAppRepoLayer.Interfaces;

namespace QuantityMeasurementAppRepoLayer.Implementations;

/// <summary>
/// UC18: Entity Framework Core repository for quantity measurement records.
/// All query methods are scoped to a <c>userId</c> so users can only
/// see and count their own operations.
/// </summary>
public class EFCoreQuantityMeasurementRepository : IQuantityMeasurementRepository
{
    private readonly ApplicationDbContext _context;

    public EFCoreQuantityMeasurementRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        Console.WriteLine("[EFCoreRepository] Initialized — using Entity Framework Core.");
    }

    // ── Save ──────────────────────────────────────────────────────────

    public void Save(QuantityMeasurementEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            if (entity.CreatedAt == default)
                entity.CreatedAt = DateTime.UtcNow;

            _context.QuantityMeasurements.Add(entity);
            _context.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("EFCoreRepository: error saving entity to database.", ex);
        }
    }

    // ── GetAll ────────────────────────────────────────────────────────

    /// <summary>Returns all records for <paramref name="userId"/>, newest first.</summary>
    public List<QuantityMeasurementEntity> GetAll(long userId)
    {
        try
        {
            return _context.QuantityMeasurements
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception("EFCoreRepository: error retrieving all records.", ex);
        }
    }

    // ── GetByOperation ────────────────────────────────────────────────

    /// <summary>
    /// Returns records for <paramref name="userId"/> where <c>Operation</c> matches
    /// <paramref name="operation"/> (case-insensitive), newest first.
    /// </summary>
    public List<QuantityMeasurementEntity> GetByOperation(string operation, long userId)
    {
        if (string.IsNullOrWhiteSpace(operation))
            throw new ArgumentException("Operation filter cannot be null or empty.", nameof(operation));

        try
        {
            string upper = operation.ToUpperInvariant();
            return _context.QuantityMeasurements
                .Where(e => e.UserId == userId && e.Operation.ToUpper() == upper)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"EFCoreRepository: error filtering by operation '{operation}'.", ex);
        }
    }

    // ── GetByMeasurementType ──────────────────────────────────────────

    /// <summary>
    /// Returns records for <paramref name="userId"/> where either operand's JSON
    /// contains <paramref name="measurementType"/> (evaluated client-side).
    /// </summary>
    public List<QuantityMeasurementEntity> GetByMeasurementType(string measurementType, long userId)
    {
        if (string.IsNullOrWhiteSpace(measurementType))
            throw new ArgumentException("MeasurementType filter cannot be null or empty.",
                                        nameof(measurementType));

        try
        {
            string upper = measurementType.ToUpperInvariant();
            return _context.QuantityMeasurements
                .Where(e => e.UserId == userId)
                .AsEnumerable()                      // switch to client-side for JSON content check
                .Where(e =>
                    (e.Operand1 != null &&
                     e.Operand1.ToString()!.Contains(upper, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Operand2 != null &&
                     e.Operand2.ToString()!.Contains(upper, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"EFCoreRepository: error filtering by measurementType '{measurementType}'.", ex);
        }
    }

    // ── GetTotalCount ─────────────────────────────────────────────────

    /// <summary>Returns the total number of records belonging to <paramref name="userId"/>.</summary>
    public int GetTotalCount(long userId)
    {
        try { return _context.QuantityMeasurements.Count(e => e.UserId == userId); }
        catch (Exception ex)
        {
            throw new Exception("EFCoreRepository: error counting records.", ex);
        }
    }

    // ── DeleteAll ─────────────────────────────────────────────────────

    /// <summary>Deletes every record from the table (admin operation).</summary>
    public void DeleteAll()
    {
        try
        {
            _context.QuantityMeasurements.RemoveRange(_context.QuantityMeasurements);
            _context.SaveChanges();
            Console.WriteLine("[EFCoreRepository] All measurements deleted.");
        }
        catch (Exception ex)
        {
            throw new Exception("EFCoreRepository: error deleting all records.", ex);
        }
    }

    // ── CloseResources ────────────────────────────────────────────────

    public void CloseResources()
        => Console.WriteLine("[EFCoreRepository] CloseResources — EF Core manages lifecycle.");
}
