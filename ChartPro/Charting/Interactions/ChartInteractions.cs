using ScottPlot;
using ScottPlot.WinForms;
using System.Drawing;
using System.Text.Json;
using ChartPro.Charting.Models;

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
    
    // Track drawn shapes with their metadata
    private readonly List<(IPlottable plottable, ShapeAnnotation metadata)> _drawnShapes = new();

    // Snap/magnet state
    private bool _snapEnabled = false;
    private SnapMode _snapMode = SnapMode.None;
    private bool _shiftKeyPressed = false;

    public ChartDrawMode CurrentDrawMode => _currentDrawMode;
    public bool IsAttached => _isAttached;
    public bool SnapEnabled 
    { 
        get => _snapEnabled; 
        set => _snapEnabled = value; 
    }
    public SnapMode SnapMode 
    { 
        get => _snapMode; 
        set => _snapMode = value; 
    }

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
        _formsPlot.KeyDown += OnKeyDown;
        _formsPlot.KeyUp += OnKeyUp;

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

    #region Event Handlers

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.ShiftKey)
        {
            _shiftKeyPressed = true;
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.ShiftKey)
        {
            _shiftKeyPressed = false;
        }
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null || _currentDrawMode == ChartDrawMode.None)
            return;

        if (e.Button == MouseButtons.Left)
        {
            // Store the starting coordinates with snap applied
            var coords = _formsPlot.Plot.GetCoordinates(e.X, e.Y);
            _drawStartCoordinates = ApplySnap(coords);
        }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null || _currentDrawMode == ChartDrawMode.None)
            return;

        if (_drawStartCoordinates == null)
            return;

        // Get current mouse coordinates with snap applied
        var currentCoordinates = _formsPlot.Plot.GetCoordinates(e.X, e.Y);
        currentCoordinates = ApplySnap(currentCoordinates);

        // Update preview
        UpdatePreview(_drawStartCoordinates.Value, currentCoordinates);
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null || _currentDrawMode == ChartDrawMode.None)
            return;

        if (e.Button == MouseButtons.Left && _drawStartCoordinates != null)
        {
            // Get final coordinates with snap applied
            var endCoordinates = _formsPlot.Plot.GetCoordinates(e.X, e.Y);
            endCoordinates = ApplySnap(endCoordinates);

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

    #region Snap Logic

    /// <summary>
    /// Applies snap logic to the given coordinates based on snap mode and Shift key state.
    /// </summary>
    private Coordinates ApplySnap(Coordinates coords)
    {
        // Check if snap is enabled (either via property or Shift key)
        bool shouldSnap = _snapEnabled || _shiftKeyPressed;
        
        if (!shouldSnap || _snapMode == SnapMode.None)
            return coords;

        return _snapMode switch
        {
            SnapMode.Price => SnapToPrice(coords),
            SnapMode.CandleOHLC => SnapToCandleOHLC(coords),
            _ => coords
        };
    }

    /// <summary>
    /// Snaps coordinates to rounded price levels.
    /// </summary>
    private Coordinates SnapToPrice(Coordinates coords)
    {
        if (_formsPlot == null)
            return coords;

        // Get the current price range visible on the chart
        var yAxis = _formsPlot.Plot.Axes.Left;
        var yRange = yAxis.Max - yAxis.Min;

        // Determine appropriate grid spacing based on visible range
        double gridSize = CalculatePriceGridSize(yRange);

        // Snap Y coordinate to nearest grid line
        double snappedY = Math.Round(coords.Y / gridSize) * gridSize;

        // For time axis, snap to visible candle positions if available
        double snappedX = coords.X;
        if (_boundCandles != null && _boundCandles.Count > 0)
        {
            // Create metadata for this shape
            var metadata = CreateShapeMetadata(_currentDrawMode, start, end);
            
            // Track the shape
            _drawnShapes.Add((plottable, metadata));
            
            _formsPlot.Plot.Add.Plottable(plottable);
            _formsPlot.Refresh();
        }

        return new Coordinates(snappedX, snappedY);
    }

    /// <summary>
    /// Calculates appropriate price grid size based on visible range.
    /// </summary>
    private double CalculatePriceGridSize(double range)
    {
        // Use logarithmic scale for grid sizing
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(range)));
        double normalized = range / magnitude;

        if (normalized < 2)
            return magnitude * 0.2;
        else if (normalized < 5)
            return magnitude * 0.5;
        else
            return magnitude;
    }

    /// <summary>
    /// Snaps coordinates to the nearest candle's OHLC values.
    /// </summary>
    private Coordinates SnapToCandleOHLC(Coordinates coords)
    {
        if (_boundCandles == null || _boundCandles.Count == 0)
            return coords;

        // Find the nearest candle by time (X coordinate)
        var nearestCandle = FindNearestCandle(coords.X);
        if (nearestCandle == null)
            return coords;

        // Find the closest OHLC value to the Y coordinate
        double[] ohlcValues = new[] 
        { 
            nearestCandle.Value.Open, 
            nearestCandle.Value.High, 
            nearestCandle.Value.Low, 
            nearestCandle.Value.Close 
        };

        double closestPrice = ohlcValues
            .OrderBy(price => Math.Abs(price - coords.Y))
            .First();

        double snappedX = nearestCandle.Value.DateTime.ToOADate();

        return new Coordinates(snappedX, closestPrice);
    }

    /// <summary>
    /// Finds the nearest candle to the given X coordinate (time).
    /// </summary>
    private OHLC? FindNearestCandle(double x)
    {
        if (_boundCandles == null || _boundCandles.Count == 0)
            return null;

        var targetTime = DateTime.FromOADate(x);
        
        return _boundCandles
            .OrderBy(candle => Math.Abs((candle.DateTime - targetTime).TotalSeconds))
            .FirstOrDefault();
    }

    /// <summary>
    /// Snaps X coordinate to the nearest candle time.
    /// </summary>
    private double SnapToNearestCandleTime(double x)
    {
        var nearestCandle = FindNearestCandle(x);
        return nearestCandle?.DateTime.ToOADate() ?? x;
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

    #region Shape Persistence

    private ShapeAnnotation CreateShapeMetadata(ChartDrawMode drawMode, Coordinates start, Coordinates end)
    {
        var metadata = new ShapeAnnotation
        {
            ShapeType = drawMode.ToString(),
            X1 = start.X,
            Y1 = start.Y,
            X2 = end.X,
            Y2 = end.Y
        };

        // Set colors and styles based on shape type
        switch (drawMode)
        {
            case ChartDrawMode.TrendLine:
                metadata.LineColor = "#0000FF"; // Blue
                metadata.LineWidth = 2;
                break;
            case ChartDrawMode.HorizontalLine:
                metadata.LineColor = "#008000"; // Green
                metadata.LineWidth = 2;
                break;
            case ChartDrawMode.VerticalLine:
                metadata.LineColor = "#FFA500"; // Orange
                metadata.LineWidth = 2;
                break;
            case ChartDrawMode.Rectangle:
                metadata.LineColor = "#800080"; // Purple
                metadata.LineWidth = 2;
                metadata.FillColor = "#800080";
                metadata.FillAlpha = 25;
                break;
            case ChartDrawMode.Circle:
                metadata.LineColor = "#00FFFF"; // Cyan
                metadata.LineWidth = 2;
                metadata.FillColor = "#00FFFF";
                metadata.FillAlpha = 25;
                break;
            case ChartDrawMode.FibonacciRetracement:
                metadata.LineColor = "#FFD700"; // Gold
                metadata.LineWidth = 2;
                break;
        }

        return metadata;
    }

    public void SaveShapesToFile(string filePath)
    {
        var annotations = new ChartAnnotations
        {
            Version = 1,
            Shapes = _drawnShapes.Select(s => s.metadata).ToList()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(annotations, options);
        File.WriteAllText(filePath, json);
    }

    public void LoadShapesFromFile(string filePath)
    {
        if (_formsPlot == null)
            throw new InvalidOperationException("Chart is not attached. Call Attach() first.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Annotations file not found.", filePath);

        var json = File.ReadAllText(filePath);
        var annotations = JsonSerializer.Deserialize<ChartAnnotations>(json);

        if (annotations == null || annotations.Shapes == null)
            return;

        // Clear existing shapes
        foreach (var (plottable, _) in _drawnShapes)
        {
            _formsPlot.Plot.Remove(plottable);
        }
        _drawnShapes.Clear();

        // Load and recreate each shape
        foreach (var shape in annotations.Shapes)
        {
            var start = new Coordinates(shape.X1, shape.Y1);
            var end = new Coordinates(shape.X2, shape.Y2);

            IPlottable? plottable = null;

            // Parse the shape type and create the appropriate plottable
            if (Enum.TryParse<ChartDrawMode>(shape.ShapeType, out var drawMode))
            {
                plottable = drawMode switch
                {
                    ChartDrawMode.TrendLine => CreateTrendLine(start, end),
                    ChartDrawMode.HorizontalLine => CreateHorizontalLine(start, end),
                    ChartDrawMode.VerticalLine => CreateVerticalLine(start, end),
                    ChartDrawMode.Rectangle => CreateRectangle(start, end),
                    ChartDrawMode.Circle => CreateCircle(start, end),
                    ChartDrawMode.FibonacciRetracement => CreateFibonacci(start, end),
                    _ => null
                };

                if (plottable != null)
                {
                    _drawnShapes.Add((plottable, shape));
                    _formsPlot.Plot.Add.Plottable(plottable);
                }
            }
        }

        _formsPlot.Refresh();
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
                _formsPlot.KeyDown -= OnKeyDown;
                _formsPlot.KeyUp -= OnKeyUp;
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
