namespace QuantityMeasurementConsoleApp;

/// <summary>Entry point for the UC17 console application.</summary>
// This starts the command-line (Console) version of our app.
public class Program
{
    public static void Main(string[] args)
    {
        QuantityMeasurementApp app = QuantityMeasurementApp.GetInstance();

        try
        {
            app.Start();
            app.ReportAllMeasurements();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Program] Unhandled error: " + ex.Message);
        }
        finally
        {
            app.CloseResources();
        }
    }
}
