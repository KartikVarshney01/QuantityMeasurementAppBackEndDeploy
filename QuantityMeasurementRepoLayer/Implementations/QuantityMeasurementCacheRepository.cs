using System.Text.Json;
using QuantityMeasurementAppModelLayer.Entities;
using QuantityMeasurementAppRepoLayer.Interfaces;

namespace QuantityMeasurementAppRepoLayer.Implementations;

/// <summary>
/// UC18: In-memory + JSON-file cache repository.
/// Used as automatic fallback when SQL Server is unavailable and as the
/// backing store for the console app (no database required).
/// <para>
/// UC18 note: all query methods now accept a <c>userId</c> parameter to match
/// the updated <see cref="IQuantityMeasurementRepository"/> contract.
/// When <c>userId</c> is 0 (console app / tests) ALL records are returned —
/// the console app has no user concept so no filtering is applied.
/// </para>
/// Thread-safe via a private lock object.
/// </summary>
public class QuantityMeasurementCacheRepository : IQuantityMeasurementRepository
{
    private static QuantityMeasurementCacheRepository? _instance;
    private static readonly object _lock = new();

    private readonly List<QuantityMeasurementEntity> _cache = new();

    private static readonly string _filePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "QuantityMeasurement_cache.json");

    private QuantityMeasurementCacheRepository()
    {
        LoadFromDisk();
        Console.WriteLine("[CacheRepository] Initialized. Backup file: " + _filePath);
    }

    // ── Singleton ─────────────────────────────────────────────────────

    public static QuantityMeasurementCacheRepository GetInstance()
    {
        if (_instance is null)
            lock (_lock)
                _instance ??= new QuantityMeasurementCacheRepository();
        return _instance;
    }

    /// <summary>Resets the singleton — use only in unit tests.</summary>
    public static void ResetForTesting()
    {
        lock (_lock) { _instance = null; }
    }

    // ── Save ──────────────────────────────────────────────────────────

    public void Save(QuantityMeasurementEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.CreatedAt == default)
            entity.CreatedAt = DateTime.UtcNow;

        lock (_lock) { _cache.Add(entity); }
        SaveToDisk();
    }

    // ── GetAll ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns records for <paramref name="userId"/>, newest first.
    /// When <paramref name="userId"/> is 0, returns ALL records (console app usage).
    /// </summary>
    public List<QuantityMeasurementEntity> GetAll(long userId)
    {
        lock (_lock)
        {
            var query = userId == 0
                ? _cache
                : _cache.Where(e => e.UserId == userId);

            return query.OrderByDescending(e => e.CreatedAt).ToList();
        }
    }

    // ── GetByOperation ────────────────────────────────────────────────

    public List<QuantityMeasurementEntity> GetByOperation(string operation, long userId)
    {
        lock (_lock)
        {
            var query = userId == 0
                ? _cache.AsEnumerable()
                : _cache.Where(e => e.UserId == userId);

            return query
                .Where(e => e.Operation.Equals(operation, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }
    }

    // ── GetByMeasurementType ──────────────────────────────────────────

    public List<QuantityMeasurementEntity> GetByMeasurementType(string measurementType, long userId)
    {
        lock (_lock)
        {
            var query = userId == 0
                ? _cache.AsEnumerable()
                : _cache.Where(e => e.UserId == userId);

            return query
                .Where(e =>
                    (e.Operand1?.ToString()?.Contains(measurementType,
                        StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.Operand2?.ToString()?.Contains(measurementType,
                        StringComparison.OrdinalIgnoreCase) ?? false))
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }
    }

    // ── GetTotalCount ─────────────────────────────────────────────────

    public int GetTotalCount(long userId)
    {
        lock (_lock)
            return userId == 0
                ? _cache.Count
                : _cache.Count(e => e.UserId == userId);
    }

    // ── DeleteAll ─────────────────────────────────────────────────────

    public void DeleteAll()
    {
        lock (_lock) { _cache.Clear(); }
        SaveToDisk();
        Console.WriteLine("[CacheRepository] All measurements deleted.");
    }

    // ── CloseResources ────────────────────────────────────────────────

    public void CloseResources()
        => Console.WriteLine("[CacheRepository] No external resources to release.");

    // ── Disk persistence ──────────────────────────────────────────────

    private void SaveToDisk()
    {
        try
        {
            var summaries = _cache.Select(e => e.ToString()).ToList();
            File.WriteAllText(_filePath,
                JsonSerializer.Serialize(summaries,
                    new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            Console.WriteLine("[CacheRepository] Warning: could not write to disk: " + ex.Message);
        }
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_filePath)) return;
        Console.WriteLine("[CacheRepository] Previous history file found: " + _filePath);
    }
}
