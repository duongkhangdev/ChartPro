using ScottPlot;
using ScottPlot.WinForms;
using System.Drawing;
using ChartPro.Charting.Interactions.Strategies;

namespace ChartPro.Charting.Interactions;

/// <summary>
/// DI-based service for managing chart interactions, drawing tools, and real-time updates.
/// Handles mouse events, shape drawing, Fibonacci tools, channels, and live candle updates.
/// </summary>
public class ChartInteractions : IChartInteractions
{
    private FormsPlot? _formsPlot;
    private int _pricePlotIndex;
    private ChartDrawMode _currentDrawMode = ChartDrawMode.None;
    private List<OHLC>? _boundCandles;
    private bool _isAttached;
    private bool _disposed;

    // Drawing state
    private Coordinates? _drawStartCoordinates;
    private IPlottable? _previewPlottable;

    public ChartDrawMode CurrentDrawMode => _currentDrawMode;
    public bool IsAttached => _isAttached;

    /// <summary>
    /// Attaches the interaction service to a FormsPlot control.
    /// </summary>
    public void Attach(FormsPlot formsPlot, int pricePlotIndex = 0)
    {
        if (_isAttached)
        {
            throw new InvalidOperationException("Already attached to a FormsPlot control. Call Dispose first.");
        }

        _formsPlot = formsPlot ?? throw new ArgumentNullException(nameof(formsPlot));
        _pricePlotIndex = pricePlotIndex;

        // Hook up event handlers
        _formsPlot.MouseDown += OnMouseDown;
        _formsPlot.MouseMove += OnMouseMove;
        _formsPlot.MouseUp += OnMouseUp;

        _isAttached = true;
    }

    /// <summary>
    /// Enables all chart interactions.
    /// </summary>
    public void EnableAll()
    {
        if (_formsPlot == null)
            return;

        // Enable pan/zoom and other interactions
        _formsPlot.UserInputProcessor.IsEnabled = true;
    }

    /// <summary>
    /// Disables all chart interactions.
    /// </summary>
    public void DisableAll()
    {
        if (_formsPlot == null)
            return;

        // Disable pan/zoom and other interactions
        _formsPlot.UserInputProcessor.IsEnabled = false;
    }

    /// <summary>
    /// Sets the current drawing mode.
    /// </summary>
    public void SetDrawMode(ChartDrawMode mode)
    {
        _currentDrawMode = mode;

        // Clear any preview
        ClearPreview();

        // Disable pan/zoom when in drawing mode
        if (mode != ChartDrawMode.None && _formsPlot != null)
        {
            _formsPlot.UserInputProcessor.IsEnabled = false;
        }
        else if (_formsPlot != null)
        {
            _formsPlot.UserInputProcessor.IsEnabled = true;
        }
    }

    /// <summary>
    /// Binds a list of OHLC candles for real-time updates.
    /// </summary>
    public void BindCandles(List<OHLC> candles)
    {
        _boundCandles = candles ?? throw new ArgumentNullException(nameof(candles));
        // TODO: Add candlestick plot to the chart if not already present
    }

    /// <summary>
    /// Updates the most recent candle for real-time display.
    /// </summary>
    public void UpdateLastCandle(OHLC candle)
    {
        if (_boundCandles == null || _boundCandles.Count == 0)
            return;

        // Update the last candle in the bound list
        _boundCandles[_boundCandles.Count - 1] = candle;

        // Refresh the chart
        _formsPlot?.Refresh();
    }

    /// <summary>
    /// Adds a new candle to the chart.
    /// </summary>
    public void AddCandle(OHLC candle)
    {
        if (_boundCandles == null)
            return;

        _boundCandles.Add(candle);

        // TODO: Auto-scale axes if needed
        _formsPlot?.Refresh();
    }

    #region Mouse Event Handlers

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null || _currentDrawMode == ChartDrawMode.None)
            return;

        if (e.Button == MouseButtons.Left)
        {
            // Store the starting coordinates
            _drawStartCoordinates = _formsPlot.Plot.GetCoordinates(e.X, e.Y);
        }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null || _currentDrawMode == ChartDrawMode.None)
            return;

        if (_drawStartCoordinates == null)
            return;

        // Get current mouse coordinates
        var currentCoordinates = _formsPlot.Plot.GetCoordinates(e.X, e.Y);

        // Update preview
        UpdatePreview(_drawStartCoordinates.Value, currentCoordinates);
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null || _currentDrawMode == ChartDrawMode.None)
            return;

        if (e.Button == MouseButtons.Left && _drawStartCoordinates != null)
        {
            // Get final coordinates
            var endCoordinates = _formsPlot.Plot.GetCoordinates(e.X, e.Y);

            // Finalize the shape
            FinalizeShape(_drawStartCoordinates.Value, endCoordinates);

            // Clear drawing state
            _drawStartCoordinates = null;
            ClearPreview();

            // Reset draw mode to None after completing a shape
            SetDrawMode(ChartDrawMode.None);
        }
    }

    #endregion

    #region Drawing Methods

    private void UpdatePreview(Coordinates start, Coordinates end)
    {
        if (_formsPlot == null)
            return;

        // Clear previous preview
        ClearPreview();

        // Use strategy pattern to create preview
        var strategy = DrawModeStrategyFactory.CreateStrategy(_currentDrawMode);
        if (strategy != null)
        {
            _previewPlottable = strategy.CreatePreview(start, end, _formsPlot.Plot);
            _formsPlot.Plot.Add.Plottable(_previewPlottable);
            _formsPlot.Refresh();
        }
    }

    private void ClearPreview()
    {
        if (_formsPlot == null || _previewPlottable == null)
            return;

        _formsPlot.Plot.Remove(_previewPlottable);
        _previewPlottable = null;
        _formsPlot.Refresh();
    }

    private void FinalizeShape(Coordinates start, Coordinates end)
    {
        if (_formsPlot == null)
            return;

        // Use strategy pattern to create final shape
        var strategy = DrawModeStrategyFactory.CreateStrategy(_currentDrawMode);
        if (strategy != null)
        {
            var plottable = strategy.CreateFinal(start, end, _formsPlot.Plot);
            _formsPlot.Plot.Add.Plottable(plottable);
            _formsPlot.Refresh();
        }
    }



    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Unhook event handlers to prevent memory leaks
            if (_formsPlot != null)
            {
                _formsPlot.MouseDown -= OnMouseDown;
                _formsPlot.MouseMove -= OnMouseMove;
                _formsPlot.MouseUp -= OnMouseUp;
            }

            _formsPlot = null;
            _boundCandles = null;
            _previewPlottable = null;
        }

        _isAttached = false;
        _disposed = true;
    }

    #endregion
}
