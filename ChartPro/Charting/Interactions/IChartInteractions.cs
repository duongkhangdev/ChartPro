using ScottPlot;
using ScottPlot.WinForms;

namespace ChartPro.Charting.Interactions;

/// <summary>
/// Interface for chart interaction service that manages mouse interactions, 
/// drawing modes, real-time updates, and technical analysis tools.
/// </summary>
public interface IChartInteractions : IDisposable
{
    /// <summary>
    /// Attaches the interaction service to a FormsPlot control.
    /// </summary>
    /// <param name="formsPlot">The FormsPlot control to attach to</param>
    /// <param name="pricePlotIndex">Index of the price plot (default: 0)</param>
    void Attach(FormsPlot formsPlot, int pricePlotIndex = 0);

    /// <summary>
    /// Enables all interactions on the chart.
    /// </summary>
    void EnableAll();

    /// <summary>
    /// Disables all interactions on the chart.
    /// </summary>
    void DisableAll();

    /// <summary>
    /// Sets the current drawing mode for the chart.
    /// </summary>
    /// <param name="mode">The drawing mode to set</param>
    void SetDrawMode(ChartDrawMode mode);

    /// <summary>
    /// Binds a list of OHLC candles for real-time updates.
    /// </summary>
    /// <param name="candles">List of OHLC candles</param>
    void BindCandles(List<OHLC> candles);

    /// <summary>
    /// Updates the most recent candle for real-time display.
    /// </summary>
    /// <param name="candle">The updated candle data</param>
    void UpdateLastCandle(OHLC candle);

    /// <summary>
    /// Adds a new candle to the chart.
    /// </summary>
    /// <param name="candle">The new candle to add</param>
    void AddCandle(OHLC candle);

    /// <summary>
    /// Gets the current drawing mode.
    /// </summary>
    ChartDrawMode CurrentDrawMode { get; }

    /// <summary>
    /// Gets whether the service is attached to a chart.
    /// </summary>
    bool IsAttached { get; }

    /// <summary>
    /// Gets the current mouse coordinates.
    /// </summary>
    Coordinates? CurrentMouseCoordinates { get; }

    /// <summary>
    /// Gets the current shape info (length, angle, size).
    /// </summary>
    string? CurrentShapeInfo { get; }

    /// <summary>
    /// Event fired when the drawing mode changes.
    /// </summary>
    event EventHandler<ChartDrawMode>? DrawModeChanged;

    /// <summary>
    /// Event fired when mouse coordinates change.
    /// </summary>
    event EventHandler<Coordinates>? MouseCoordinatesChanged;

    /// <summary>
    /// Event fired when shape info changes.
    /// </summary>
    event EventHandler<string>? ShapeInfoChanged;
}
