using QuantityMeasurementConsoleApp.Controllers;
using QuantityMeasurementConsoleApp.Interfaces;

namespace QuantityMeasurementConsoleApp.Menu;

/// <summary>
/// Thin <see cref="IMenu"/> implementation that delegates <see cref="Run"/>
/// to the <see cref="QuantityMeasurementController"/>.
/// </summary>
// This is the actual menu that the user sees and interacts with in the console.
public class QuantityMenu : IMenu
{
    private readonly QuantityMeasurementController _controller;

    public QuantityMenu(QuantityMeasurementController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    public void Run() => _controller.Run();
}
