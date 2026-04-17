using QuantityMeasurementAppBusinessLayer.Exceptions;
using QuantityMeasurementAppBusinessLayer.Interfaces;
using QuantityMeasurementAppModelLayer.DTOs;

namespace QuantityMeasurementConsoleApp.Controllers;

/// <summary>
/// Console application controller.
/// Drives the interactive menu loop and delegates every operation to
/// <see cref="IQuantityMeasurementService"/>. Unchanged business logic from UC16;
/// updated to use the UC17 service interface signatures
/// (Compare returns bool, Convert accepts a target-unit string).
/// </summary>
// This is the controller for the console app that handles what happens when you pick a menu option.
public class QuantityMeasurementController
{
    private readonly IQuantityMeasurementService _service;

    public QuantityMeasurementController(IQuantityMeasurementService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    // ── Main application loop ─────────────────────────────────────────

    public void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║   Quantity Measurement Application   ║");
        Console.WriteLine("║            UC17 Console              ║");
        Console.WriteLine("╚══════════════════════════════════════╝");

        bool running = true;
        while (running)
        {
            Console.WriteLine("\n===== MAIN MENU =====");
            Console.WriteLine("1. Length");
            Console.WriteLine("2. Weight");
            Console.WriteLine("3. Volume");
            Console.WriteLine("4. Temperature");
            Console.WriteLine("5. View History");
            Console.WriteLine("0. Exit");
            Console.Write("\nSelect category: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunLengthMenu();      break;
                case "2": RunWeightMenu();      break;
                case "3": RunVolumeMenu();      break;
                case "4": RunTemperatureMenu(); break;
                case "5": ShowHistory();        break;
                case "0":
                    running = false;
                    Console.WriteLine("Goodbye!");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please enter 1-5 or 0.");
                    break;
            }
        }
    }

    // ── Public Perform* methods (used by tests / future callers) ──────

    public string PerformComparison(QuantityDTO q1, QuantityDTO q2)
    {
        try
        {
            bool equal = _service.Compare(q1, q2, 0); // Pass default userId 0
            return $"Comparison Result: {(equal ? "true (quantities are equal)" : "false (quantities are not equal)")}";
        }
        catch (QuantityMeasurementException ex) { return $"[ERROR] {ex.Message}"; }
    }

    public string PerformConversion(QuantityDTO q1, string targetUnit)
    {
        try
        {
            var result = _service.Convert(q1, targetUnit, 0); // Pass default userId 0
            return $"Conversion Result: {result.Value} {result.UnitName}";
        }
        catch (QuantityMeasurementException ex) { return $"[ERROR] {ex.Message}"; }
    }

    public string PerformAddition(QuantityDTO q1, QuantityDTO q2)
    {
        try
        {
            var result = _service.Add(q1, q2, 0); // Pass default userId 0
            return $"Addition Result: {result.Value} {result.UnitName}";
        }
        catch (QuantityMeasurementException ex) { return $"[ERROR] {ex.Message}"; }
    }

    public string PerformSubtraction(QuantityDTO q1, QuantityDTO q2)
    {
        try
        {
            var result = _service.Subtract(q1, q2, 0); // Pass default userId 0
            return $"Subtraction Result: {result.Value} {result.UnitName}";
        }
        catch (QuantityMeasurementException ex) { return $"[ERROR] {ex.Message}"; }
    }

    public string PerformDivision(QuantityDTO q1, QuantityDTO q2)
    {
        try
        {
            double result = _service.Divide(q1, q2, 0); // Pass default userId 0
            return $"Division Result: {result} (scalar ratio)";
        }
        catch (QuantityMeasurementException ex) { return $"[ERROR] {ex.Message}"; }
    }

    // ── Category menus ────────────────────────────────────────────────

    private void RunLengthMenu()
    {
        Console.WriteLine("\n--- LENGTH UNITS: Feet | Inch | Yard | Centimeter ---");
        RunOperationMenu("LENGTH", supportsArithmetic: true);
    }

    private void RunWeightMenu()
    {
        Console.WriteLine("\n--- WEIGHT UNITS: Kilogram | Gram | Pound ---");
        RunOperationMenu("WEIGHT", supportsArithmetic: true);
    }

    private void RunVolumeMenu()
    {
        Console.WriteLine("\n--- VOLUME UNITS: Litre | Millilitre | Gallon ---");
        RunOperationMenu("VOLUME", supportsArithmetic: true);
    }

    private void RunTemperatureMenu()
    {
        Console.WriteLine("\n--- TEMPERATURE UNITS: Celsius | Fahrenheit | Kelvin ---");
        Console.WriteLine("(Note: Arithmetic operations are NOT supported for Temperature)");
        RunOperationMenu("TEMPERATURE", supportsArithmetic: false);
    }

    // ── Shared operation dispatcher ────────────────────────────────────

    private void RunOperationMenu(string category, bool supportsArithmetic)
    {
        string op = SelectOperation(supportsArithmetic);
        if (op == "0") return;

        try
        {
            switch (op)
            {
                case "1": // Convert
                {
                    double v = ReadValue("Enter value");
                    string u = ReadUnit("Enter unit (e.g. Feet)");
                    string t = ReadUnit("Convert to unit");
                    Console.WriteLine(PerformConversion(new QuantityDTO(v, u, category), t));
                    break;
                }
                case "2": // Compare
                {
                    var (q1, q2) = ReadTwoQuantities(category);
                    Console.WriteLine(PerformComparison(q1, q2));
                    break;
                }
                case "3": // Add
                {
                    var (q1, q2) = ReadTwoQuantities(category);
                    Console.WriteLine(PerformAddition(q1, q2));
                    break;
                }
                case "4": // Subtract
                {
                    var (q1, q2) = ReadTwoQuantities(category);
                    Console.WriteLine(PerformSubtraction(q1, q2));
                    break;
                }
                case "5": // Divide
                {
                    var (q1, q2) = ReadTwoQuantities(category);
                    Console.WriteLine(PerformDivision(q1, q2));
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // ── History display ───────────────────────────────────────────────

    private void ShowHistory()
    {
        var history = _service.GetHistory(0); // Pass default userId 0
        Console.WriteLine($"\n===== OPERATION HISTORY ({history.Count} records) =====");

        if (history.Count == 0)
        {
            Console.WriteLine("No operations recorded yet.");
            return;
        }

        for (int i = 0; i < history.Count; i++)
            Console.WriteLine($"{i + 1,3}. {history[i]}");

        Console.WriteLine("=================================================");
    }

    // ── Input helpers ─────────────────────────────────────────────────

    private static string SelectOperation(bool supportsArithmetic)
    {
        Console.WriteLine("\nSelect operation:");
        Console.WriteLine("1. Convert");
        Console.WriteLine("2. Compare");
        if (supportsArithmetic)
        {
            Console.WriteLine("3. Add");
            Console.WriteLine("4. Subtract");
            Console.WriteLine("5. Divide");
        }
        Console.WriteLine("0. Back");
        Console.Write("Choice: ");

        string? input = Console.ReadLine()?.Trim();

        if (!supportsArithmetic && input is "3" or "4" or "5")
        {
            Console.WriteLine("Arithmetic operations are not supported for Temperature.");
            return "0";
        }

        return input ?? "0";
    }

    private static double ReadValue(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine()?.Trim();
            if (double.TryParse(input, out double value)) return value;
            Console.WriteLine($"'{input}' is not a valid number. Try again.");
        }
    }

    private static string ReadUnit(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input)) return input;
            Console.WriteLine("Unit cannot be empty. Try again.");
        }
    }

    private static (QuantityDTO q1, QuantityDTO q2) ReadTwoQuantities(string category)
    {
        double v1 = ReadValue("Enter first value");
        string u1 = ReadUnit("Enter first unit");
        double v2 = ReadValue("Enter second value");
        string u2 = ReadUnit("Enter second unit");
        return (new QuantityDTO(v1, u1, category), new QuantityDTO(v2, u2, category));
    }
}
