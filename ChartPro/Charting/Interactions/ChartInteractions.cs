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

        // Create preview based on draw mode
        _previewPlottable = _currentDrawMode switch
        {
            ChartDrawMode.TrendLine => CreateTrendLinePreview(start, end),
            ChartDrawMode.HorizontalLine => CreateHorizontalLinePreview(start, end),
            ChartDrawMode.VerticalLine => CreateVerticalLinePreview(start, end),
            ChartDrawMode.Rectangle => CreateRectanglePreview(start, end),
            ChartDrawMode.Circle => CreateCirclePreview(start, end),
            ChartDrawMode.FibonacciRetracement => CreateFibonacciPreview(start, end),
            // TODO: Implement other draw modes (FibonacciExtension, Channel, Triangle, Text)
            _ => null
        };

        if (_previewPlottable != null)
        {
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

        // Create permanent plottable based on draw mode
        IPlottable? plottable = _currentDrawMode switch
        {
            ChartDrawMode.TrendLine => CreateTrendLine(start, end),
            ChartDrawMode.HorizontalLine => CreateHorizontalLine(start, end),
            ChartDrawMode.VerticalLine => CreateVerticalLine(start, end),
            ChartDrawMode.Rectangle => CreateRectangle(start, end),
            ChartDrawMode.Circle => CreateCircle(start, end),
            ChartDrawMode.FibonacciRetracement => CreateFibonacci(start, end),
            // TODO: Implement other draw modes
            _ => null
        };

        if (plottable != null)
        {
            // Create metadata for this shape
            var metadata = CreateShapeMetadata(_currentDrawMode, start, end);
            
            // Track the shape
            _drawnShapes.Add((plottable, metadata));
            
            _formsPlot.Plot.Add.Plottable(plottable);
            _formsPlot.Refresh();
        }
    }

    private IPlottable CreateTrendLinePreview(Coordinates start, Coordinates end)
    {
        var line = _formsPlot!.Plot.Add.Line(start, end);
        line.LineWidth = 1;
        line.LineColor = Colors.Gray.WithAlpha(0.5);
        return line;
    }

    private IPlottable CreateTrendLine(Coordinates start, Coordinates end)
    {
        var line = _formsPlot!.Plot.Add.Line(start, end);
        line.LineWidth = 2;
        line.LineColor = Colors.Blue;
        return line;
    }

    private IPlottable CreateHorizontalLinePreview(Coordinates start, Coordinates end)
    {
        var hLine = _formsPlot!.Plot.Add.HorizontalLine(end.Y);
        hLine.LineWidth = 1;
        hLine.LineColor = Colors.Gray.WithAlpha(0.5);
        return hLine;
    }

    private IPlottable CreateHorizontalLine(Coordinates start, Coordinates end)
    {
        var hLine = _formsPlot!.Plot.Add.HorizontalLine(end.Y);
        hLine.LineWidth = 2;
        hLine.LineColor = Colors.Green;
        return hLine;
    }

    private IPlottable CreateVerticalLinePreview(Coordinates start, Coordinates end)
    {
        var vLine = _formsPlot!.Plot.Add.VerticalLine(end.X);
        vLine.LineWidth = 1;
        vLine.LineColor = Colors.Gray.WithAlpha(0.5);
        return vLine;
    }

    private IPlottable CreateVerticalLine(Coordinates start, Coordinates end)
    {
        var vLine = _formsPlot!.Plot.Add.VerticalLine(end.X);
        vLine.LineWidth = 2;
        vLine.LineColor = Colors.Orange;
        return vLine;
    }

    private IPlottable CreateRectanglePreview(Coordinates start, Coordinates end)
    {
        var rect = _formsPlot!.Plot.Add.Rectangle(
            Math.Min(start.X, end.X),
            Math.Max(start.X, end.X),
            Math.Min(start.Y, end.Y),
            Math.Max(start.Y, end.Y));
        rect.LineWidth = 1;
        rect.LineColor = Colors.Gray.WithAlpha(0.5);
        rect.FillColor = Colors.Gray.WithAlpha(0.1);
        return rect;
    }

    private IPlottable CreateRectangle(Coordinates start, Coordinates end)
    {
        var rect = _formsPlot!.Plot.Add.Rectangle(
            Math.Min(start.X, end.X),
            Math.Max(start.X, end.X),
            Math.Min(start.Y, end.Y),
            Math.Max(start.Y, end.Y));
        rect.LineWidth = 2;
        rect.LineColor = Colors.Purple;
        rect.FillColor = Colors.Purple.WithAlpha(0.1);
        return rect;
    }

    private IPlottable CreateCirclePreview(Coordinates start, Coordinates end)
    {
        var centerX = (start.X + end.X) / 2;
        var centerY = (start.Y + end.Y) / 2;
        var radiusX = Math.Abs(end.X - start.X) / 2;
        var radiusY = Math.Abs(end.Y - start.Y) / 2;
        
        var circle = _formsPlot!.Plot.Add.Ellipse(centerX, centerY, radiusX, radiusY);
        circle.LineWidth = 1;
        circle.LineColor = Colors.Gray.WithAlpha(0.5);
        circle.FillColor = Colors.Gray.WithAlpha(0.1);
        return circle;
    }

    private IPlottable CreateCircle(Coordinates start, Coordinates end)
    {
        var centerX = (start.X + end.X) / 2;
        var centerY = (start.Y + end.Y) / 2;
        var radiusX = Math.Abs(end.X - start.X) / 2;
        var radiusY = Math.Abs(end.Y - start.Y) / 2;
        
        var circle = _formsPlot!.Plot.Add.Ellipse(centerX, centerY, radiusX, radiusY);
        circle.LineWidth = 2;
        circle.LineColor = Colors.Cyan;
        circle.FillColor = Colors.Cyan.WithAlpha(0.1);
        return circle;
    }

    private IPlottable CreateFibonacciPreview(Coordinates start, Coordinates end)
    {
        // TODO: Implement full Fibonacci retracement with levels (0.0, 0.236, 0.382, 0.5, 0.618, 0.786, 1.0)
        // For now, create a simple line as preview
        var line = _formsPlot!.Plot.Add.Line(start, end);
        line.LineWidth = 1;
        line.LineColor = Colors.Gold.WithAlpha(0.5);
        return line;
    }

    private IPlottable CreateFibonacci(Coordinates start, Coordinates end)
    {
        // TODO: Implement full Fibonacci retracement with levels and labels
        // For now, create a simple line
        var line = _formsPlot!.Plot.Add.Line(start, end);
        line.LineWidth = 2;
        line.LineColor = Colors.Gold;
        return line;
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
