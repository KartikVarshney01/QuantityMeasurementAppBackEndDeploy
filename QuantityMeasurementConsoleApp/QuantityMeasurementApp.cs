using QuantityMeasurementAppBusinessLayer.Interfaces;
using QuantityMeasurementAppBusinessLayer.Services;
using QuantityMeasurementAppModelLayer.Entities;
using QuantityMeasurementAppRepoLayer.Implementations;
using QuantityMeasurementAppRepoLayer.Interfaces;
using QuantityMeasurementConsoleApp.Controllers;
using QuantityMeasurementConsoleApp.Interfaces;
using QuantityMeasurementConsoleApp.Menu;

namespace QuantityMeasurementConsoleApp;

/// <summary>
/// UC17 Console Application bootstrapper.
/// Wires up the repository (Cache repository — the console app does not use EF Core
/// or SQL Server directly; persistence happens through the shared Cache repository),
/// service, controller, and menu using the Singleton pattern.
/// </summary>
// This file sets up all the parts of the console app so they can work together.
public class QuantityMeasurementApp
{
    private static QuantityMeasurementApp? _instance;
    private static readonly object _lock = new();

    private readonly QuantityMeasurementController _controller;
    private readonly IMenu                         _menu;
    private readonly IQuantityMeasurementRepository _repository;
    private readonly string                         _activeRepositoryType;

    private QuantityMeasurementApp()
    {
        Console.WriteLine("[App] Starting Quantity Measurement Console Application (UC17)...");

        // UC17 console app uses the Cache repository.
        // If you want SQL Server persistence from the console, replace this with
        // EFCoreQuantityMeasurementRepository wired with a manually constructed
        // ApplicationDbContext — but for UC17 the console stays cache-based.
        _repository           = QuantityMeasurementCacheRepository.GetInstance();
        _activeRepositoryType = "Cache (in-memory + JSON backup)";
        Console.WriteLine("[App] Using repository: " + _activeRepositoryType);

        IQuantityMeasurementService service =
            new QuantityMeasurementServiceImpl(_repository);

        _controller = new QuantityMeasurementController(service);
        _menu       = new QuantityMenu(_controller);

        Console.WriteLine("[App] Initialization complete.");
        Console.WriteLine();
    }

    // ── Singleton ─────────────────────────────────────────────────────

    public static QuantityMeasurementApp GetInstance()
    {
        if (_instance is null)
            lock (_lock)
                _instance ??= new QuantityMeasurementApp();
        return _instance;
    }

    // ── Lifecycle ─────────────────────────────────────────────────────

    public void Start() => _menu.Run();

    public void ReportAllMeasurements()
    {
        Console.WriteLine("\n========== Measurement History ==========");
        Console.WriteLine("Repository : " + _activeRepositoryType);

        List<QuantityMeasurementEntity> all = _repository.GetAll(0);
        Console.WriteLine("Total records: " + all.Count);

        for (int i = 0; i < all.Count; i++)
            Console.WriteLine((i + 1) + ". " + all[i]);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("Total count : " + _repository.GetTotalCount(0));
        Console.WriteLine("=========================================\n");
    }

    public void DeleteAllMeasurements()
    {
        Console.WriteLine("[App] Deleting all measurements...");
        _repository.DeleteAll();
        Console.WriteLine("[App] Done. Remaining records: " + _repository.GetTotalCount(0));
    }

    public void CloseResources()
    {
        Console.WriteLine("[App] Closing resources...");
        _repository.CloseResources();
        Console.WriteLine("[App] Resources closed.");
    }
}
