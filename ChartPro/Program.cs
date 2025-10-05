using ChartPro.Charting.Interactions;
using ChartPro.Charting.ShapeManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChartPro;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Build the DI container
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register the shape manager service
                services.AddTransient<IShapeManager, ShapeManager>();

                // Register the chart interactions service
                services.AddTransient<IChartInteractions, ChartInteractions>();

                // Register the main form
                services.AddTransient<MainForm>();
            })
            .Build();

        // Resolve and run the main form
        var mainForm = host.Services.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }
}
