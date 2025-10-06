using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using ChartPro.Charting.Models;
using ChartPro.Charting.ShapeManagement;
using ChartPro.Charting.Interactions.Strategies;
using ScottPlot;
using ScottPlot.WinForms;

namespace ChartPro.Charting.Interactions;

/// <summary>
/// DI-based service for managing chart interactions, drawing tools, and real-time updates.
/// Handles mouse events, shape drawing, Fibonacci tools, channels, and live candle updates.
/// </summary>
public class ChartInteractions : IChartInteractions, IDisposable
{
    private readonly IShapeManager _shapeManager;
    private FormsPlot? _formsPlot;
    private int _pricePlotIndex;
    private ChartDrawMode _currentDrawMode = ChartDrawMode.None;
    private List<OHLC>? _boundCandles;
    private bool _isAttached;
    private bool _disposed;
    private bool _snapEnabled;
    private SnapMode _snapMode = SnapMode.None;
    private bool _shiftKeyPressed;

    // Drawing state
    private Coordinates? _drawStartCoordinates;
    private IPlottable? _previewPlottable;
    private Coordinates? _currentMouseCoordinates;
    private string? _currentShapeInfo;

    // Persistence state (for save/load)
    private readonly List<(IPlottable Plottable, ShapeAnnotation metadata)> _drawnShapes = new();

    // Public properties
    public ChartDrawMode CurrentDrawMode => _currentDrawMode;
    public bool IsAttached => _isAttached;
    public IShapeManager ShapeManager => _shapeManager;
    public Coordinates? CurrentMouseCoordinates => _currentMouseCoordinates;
    public string? CurrentShapeInfo => _currentShapeInfo;
    public bool SnapEnabled { get => _snapEnabled; set => _snapEnabled = value; }
    public SnapMode SnapMode { get => _snapMode; set => _snapMode = value; }

    // Events
    public event EventHandler<ChartDrawMode>? DrawModeChanged;
    public event EventHandler<Coordinates>? MouseCoordinatesChanged;
    public event EventHandler<string>? ShapeInfoChanged;

    public ChartInteractions(IShapeManager shapeManager)
    {
        _shapeManager = shapeManager ?? throw new ArgumentNullException(nameof(shapeManager));
    }

    /// <summary>
    /// Attaches the interaction service to a FormsPlot control.
    /// </summary>
    public void Attach(FormsPlot formsPlot, int pricePlotIndex = 0)
    {
        if (_isAttached)
            throw new InvalidOperationException("Already attached to a FormsPlot control. Call Dispose first.");

        _formsPlot = formsPlot ?? throw new ArgumentNullException(nameof(formsPlot));
        _pricePlotIndex = pricePlotIndex;

        // Attach shape manager
        _shapeManager.Attach(_formsPlot);

        // Hook up event handlers
        _formsPlot.MouseDown += OnMouseDown;
        _formsPlot.MouseMove += OnMouseMove;
        _formsPlot.MouseUp += OnMouseUp;
        _formsPlot.KeyDown += OnKeyDown;
        _formsPlot.KeyUp += OnKeyUp;

        _isAttached = true;
    }

    public void EnableAll()
    {
        if (_formsPlot == null)
            return;

        _formsPlot.UserInputProcessor.IsEnabled = true;
    }

    public void DisableAll()
    {
        if (_formsPlot == null)
            return;

        _formsPlot.UserInputProcessor.IsEnabled = false;
    }

    public void SetDrawMode(ChartDrawMode mode)
    {
        _currentDrawMode = mode;

        // Clear any preview
        ClearPreview();

        // Clear shape info when changing modes
        UpdateShapeInfo(null);

        // Disable pan/zoom when in drawing mode
        if (mode != ChartDrawMode.None && _formsPlot != null)
            _formsPlot.UserInputProcessor.IsEnabled = false;
        else if (_formsPlot != null)
            _formsPlot.UserInputProcessor.IsEnabled = true;

        // Fire mode changed event
        DrawModeChanged?.Invoke(this, mode);
    }

    public void BindCandles(List<OHLC> candles)
    {
        _boundCandles = candles ?? throw new ArgumentNullException(nameof(candles));
        // TODO: Add candlestick plot to the chart if not already present
    }

    public void UpdateLastCandle(OHLC candle)
    {
        if (_boundCandles == null || _boundCandles.Count == 0)
            return;

        _boundCandles[^1] = candle;
        _formsPlot?.Refresh();
    }

    public void AddCandle(OHLC candle)
    {
        if (_boundCandles == null)
            return;

        _boundCandles.Add(candle);
        _formsPlot?.Refresh();
    }

    #region Event Handlers

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Shift && !_shiftKeyPressed)
        {
            _shiftKeyPressed = true;
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (!e.Shift && _shiftKeyPressed)
        {
            _shiftKeyPressed = false;
        }
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null)
            return;

        if (_currentDrawMode == ChartDrawMode.None)
        {
            if (e.Button == MouseButtons.Left)
                HandleShapeSelection(e.X, e.Y, Control.ModifierKeys);
            return;
        }

        if (e.Button == MouseButtons.Left)
        {
            var coords = _formsPlot.Plot.GetCoordinates(e.X, e.Y);
            _drawStartCoordinates = ApplySnap(coords);
        }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null)
            return;

        var currentCoordinates = _formsPlot.Plot.GetCoordinates(e.X, e.Y);
        UpdateMouseCoordinates(currentCoordinates);

        if (_currentDrawMode == ChartDrawMode.None)
            return;

        if (_drawStartCoordinates == null)
            return;

        var snappedCoordinates = ApplySnap(currentCoordinates);
        UpdatePreview(_drawStartCoordinates.Value, snappedCoordinates);
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null || _currentDrawMode == ChartDrawMode.None)
            return;

        if (e.Button == MouseButtons.Left && _drawStartCoordinates != null)
        {
            var endCoordinates = _formsPlot.Plot.GetCoordinates(e.X, e.Y);
            endCoordinates = ApplySnap(endCoordinates);

            FinalizeShape(_drawStartCoordinates.Value, endCoordinates);

            _drawStartCoordinates = null;
            ClearPreview();
            SetDrawMode(ChartDrawMode.None);
        }
    }

    #endregion

    #region Status Updates

    private void UpdateMouseCoordinates(Coordinates coordinates)
    {
        _currentMouseCoordinates = coordinates;
        MouseCoordinatesChanged?.Invoke(this, coordinates);
    }

    private void UpdateShapeInfo(string? info)
    {
        _currentShapeInfo = info;
        ShapeInfoChanged?.Invoke(this, info ?? string.Empty);
    }

    private string CalculateShapeInfo(Coordinates start, Coordinates end)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        var angle = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;

        return _currentDrawMode switch
        {
            ChartDrawMode.TrendLine => $"Length: {distance:F2}, Angle: {angle:F1}°",
            ChartDrawMode.HorizontalLine => $"Price: {end.Y:F2}",
            ChartDrawMode.VerticalLine => $"Time: {end.X:F2}",
            ChartDrawMode.Rectangle => $"Width: {Math.Abs(deltaX):F2}, Height: {Math.Abs(deltaY):F2}",
            ChartDrawMode.Circle => $"RadiusX: {Math.Abs(deltaX) / 2:F2}, RadiusY: {Math.Abs(deltaY) / 2:F2}",
            ChartDrawMode.FibonacciRetracement => $"Range: {Math.Abs(deltaY):F2}",
            ChartDrawMode.FibonacciExtension => $"Range: {Math.Abs(deltaY):F2}",
            _ => string.Empty
        };
    }

    #endregion

    #region Helpers

    private Coordinates ApplySnap(Coordinates c)
    {
        // Check if snap is enabled (either via checkbox or Shift key)
        bool shouldSnap = _snapEnabled || _shiftKeyPressed;
        if (!shouldSnap)
            return c;

        // Apply the appropriate snap mode
        return _snapMode switch
        {
            SnapMode.Price => SnapToPrice(c),
            SnapMode.CandleOHLC => SnapToCandleOHLC(c),
            _ => c
        };
    }

    private Coordinates SnapToPrice(Coordinates coords)
    {
        if (_formsPlot == null)
            return coords;

        // Get visible axis limits
        var xAxis = _formsPlot.Plot.Axes.Bottom;
        var yAxis = _formsPlot.Plot.Axes.Left;
        
        var xRange = xAxis.Max - xAxis.Min;
        var yRange = yAxis.Max - yAxis.Min;

        // Calculate appropriate grid size for Y (price) axis
        double priceGridSize = CalculatePriceGridSize(yRange);
        
        // Snap Y to grid
        double snappedY = Math.Round(coords.Y / priceGridSize) * priceGridSize;

        // Snap X to nearest candle time if candles are available
        double snappedX = coords.X;
        if (_boundCandles != null && _boundCandles.Count > 0)
        {
            var nearestCandle = FindNearestCandle(coords.X);
            if (nearestCandle.HasValue)
            {
                snappedX = nearestCandle.Value.DateTime.ToOADate();
            }
        }

        return new Coordinates(snappedX, snappedY);
    }

    private Coordinates SnapToCandleOHLC(Coordinates coords)
    {
        if (_boundCandles == null || _boundCandles.Count == 0)
            return coords;

        // Find nearest candle by time
        var nearestCandle = FindNearestCandle(coords.X);
        if (!nearestCandle.HasValue)
            return coords;

        var candle = nearestCandle.Value;
        
        // Get OHLC values
        double[] ohlcValues = { candle.Open, candle.High, candle.Low, candle.Close };
        
        // Find closest OHLC value to current Y coordinate
        double closestPrice = ohlcValues[0];
        double minDistance = Math.Abs(coords.Y - closestPrice);
        
        for (int i = 1; i < ohlcValues.Length; i++)
        {
            double distance = Math.Abs(coords.Y - ohlcValues[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPrice = ohlcValues[i];
            }
        }

        // Snap to candle time and closest OHLC value
        return new Coordinates(candle.DateTime.ToOADate(), closestPrice);
    }

    private double CalculatePriceGridSize(double range)
    {
        // Calculate appropriate grid size based on visible range
        // Use logarithmic scaling for nice round numbers
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(range)));
        
        // Determine multiplier for nice grid spacing (1, 2, 5, or 10)
        double normalizedRange = range / magnitude;
        double gridSize;
        
        if (normalizedRange <= 1.5)
            gridSize = magnitude * 0.1;
        else if (normalizedRange <= 3)
            gridSize = magnitude * 0.2;
        else if (normalizedRange <= 7)
            gridSize = magnitude * 0.5;
        else
            gridSize = magnitude;

        return gridSize;
    }

    private OHLC? FindNearestCandle(double time)
    {
        if (_boundCandles == null || _boundCandles.Count == 0)
            return null;

        // Find candle with closest time to the given coordinate
        OHLC? nearestCandle = null;
        double minDistance = double.MaxValue;

        foreach (var candle in _boundCandles)
        {
            double candleTime = candle.DateTime.ToOADate();
            double distance = Math.Abs(time - candleTime);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCandle = candle;
            }
        }

        return nearestCandle;
    }

    #endregion

    #region Drawing Methods

    private void UpdatePreview(Coordinates start, Coordinates end)
    {
        if (_formsPlot == null)
            return;

        ClearPreview();
        var shapeInfo = CalculateShapeInfo(start, end);
        UpdateShapeInfo(shapeInfo);

        var strategy = DrawModeStrategyFactory.CreateStrategy(_currentDrawMode);
        _previewPlottable = strategy?.CreatePreview(start, end, _formsPlot.Plot);

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

        var strategy = DrawModeStrategyFactory.CreateStrategy(_currentDrawMode);
        var plottable = strategy?.CreateFinal(start, end, _formsPlot.Plot);

        if (plottable != null)
        {
            _formsPlot.Plot.Add.Plottable(plottable);
            
            // Create DrawnShape with metadata
            var drawnShape = new Charting.Shapes.DrawnShape(plottable, _currentDrawMode);
            _shapeManager.AddShape(drawnShape);

            var metadata = CreateShapeMetadata(_currentDrawMode, start, end);
            _drawnShapes.Add((plottable, metadata));

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

        switch (drawMode)
        {
            case ChartDrawMode.TrendLine:
                metadata.LineColor = "#0000FF";
                metadata.LineWidth = 2;
                break;
            case ChartDrawMode.HorizontalLine:
                metadata.LineColor = "#008000";
                metadata.LineWidth = 2;
                break;
            case ChartDrawMode.VerticalLine:
                metadata.LineColor = "#FFA500";
                metadata.LineWidth = 2;
                break;
            case ChartDrawMode.Rectangle:
                metadata.LineColor = "#800080";
                metadata.LineWidth = 2;
                metadata.FillColor = "#800080";
                metadata.FillAlpha = 25;
                break;
            case ChartDrawMode.Circle:
                metadata.LineColor = "#00FFFF";
                metadata.LineWidth = 2;
                metadata.FillColor = "#00FFFF";
                metadata.FillAlpha = 25;
                break;
            case ChartDrawMode.FibonacciRetracement:
            case ChartDrawMode.FibonacciExtension:
                metadata.LineColor = "#FFD700";
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

        var options = new JsonSerializerOptions { WriteIndented = true };
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

        foreach (var (plottable, _) in _drawnShapes)
            _formsPlot.Plot.Remove(plottable);
        _drawnShapes.Clear();

        foreach (var shape in annotations.Shapes)
        {
            var start = new Coordinates(shape.X1, shape.Y1);
            var end = new Coordinates(shape.X2, shape.Y2);

            if (Enum.TryParse<ChartDrawMode>(shape.ShapeType, out var drawMode))
            {
                var strategy = DrawModeStrategyFactory.CreateStrategy(drawMode);
                var plottable = strategy?.CreateFinal(start, end, _formsPlot.Plot);
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

    #region Shape Selection

    private void HandleShapeSelection(int pixelX, int pixelY, Keys modifiers)
    {
        if (_formsPlot == null)
            return;

        // Check if Ctrl key is pressed to add to selection
        var addToSelection = (modifiers & Keys.Control) == Keys.Control;

        // Try to select a shape at the clicked location
        var selectedShape = _shapeManager.SelectShapeAt(pixelX, pixelY, addToSelection);

        if (selectedShape != null)
        {
            // Shape was selected, update UI if needed
            _formsPlot.Refresh();
        }
        else if (!addToSelection)
        {
            // No shape found and not adding to selection, clear selection
            _shapeManager.ClearSelection();
        }
    }

    #endregion

    #region Undo/Redo/Delete Operations

    public bool Undo()
    {
        if (!_shapeManager.CanUndo)
            return false;

        var result = _shapeManager.Undo();
        _formsPlot?.Refresh();
        return result;
    }

    public bool Redo()
    {
        if (!_shapeManager.CanRedo)
            return false;

        var result = _shapeManager.Redo();
        _formsPlot?.Refresh();
        return result;
    }

    public void DeleteSelectedShapes()
    {
        _shapeManager.DeleteSelectedShapes();
        _formsPlot?.Refresh();
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
            if (_formsPlot != null)
            {
                _formsPlot.MouseDown -= OnMouseDown;
                _formsPlot.MouseMove -= OnMouseMove;
                _formsPlot.MouseUp -= OnMouseUp;
                _formsPlot.KeyDown -= OnKeyDown;
                _formsPlot.KeyUp -= OnKeyUp;
            }

            _shapeManager?.Dispose();
            _formsPlot = null;
            _boundCandles = null;
            _previewPlottable = null;
        }

        _isAttached = false;
        _disposed = true;
    }

    #endregion
}
