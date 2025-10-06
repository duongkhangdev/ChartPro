using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using ChartPro.Charting.Models;
using ChartPro.Charting.ShapeManagement;
using ChartPro.Charting.Shapes;
using ChartPro.Charting.Interactions.Strategies;
using ScottPlot;
using ScottPlot.WinForms;
using System.Drawing;

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

    /// <summary>
    /// Enables all chart interactions.
    /// </summary>
    public void EnableAll()
    {
        if (_formsPlot == null)
            return;

        _formsPlot.UserInputProcessor.IsEnabled = true;
    }

    /// <summary>
    /// Disables all chart interactions.
    /// </summary>
    public void DisableAll()
    {
        if (_formsPlot == null)
            return;

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

        _boundCandles[^1] = candle;
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
        _formsPlot?.Refresh();
    }

    #region Event Handlers

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.ShiftKey)
            _shiftKeyPressed = true;
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.ShiftKey)
            _shiftKeyPressed = false;
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null)
            return;

        if (_currentDrawMode == ChartDrawMode.None)
        {
            // Selection mode - try to select a shape
            if (e.Button == MouseButtons.Left)
                HandleShapeSelection(e.X, e.Y, Control.ModifierKeys);
            return;
        }

        if (e.Button == MouseButtons.Left)
        {
            // Store the starting coordinates with snap applied
            var coords = _formsPlot.Plot.GetCoordinates(e.X, e.Y);
            _drawStartCoordinates = ApplySnap(coords);
        }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_formsPlot == null)
            return;

        // Always update mouse coordinates
        var currentCoordinates = _formsPlot.Plot.GetCoordinates(e.X, e.Y);
        UpdateMouseCoordinates(currentCoordinates);

        if (_currentDrawMode == ChartDrawMode.None)
            return;

        if (_drawStartCoordinates == null)
            return;

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
        // Placeholder for snapping logic (e.g., hold Shift to constrain angles)
        return c;
    }

    #endregion

    #region Drawing Methods

    private void UpdatePreview(Coordinates start, Coordinates end)
    {
        if (_formsPlot == null)
            return;

        // Clear previous preview
        ClearPreview();

        // Calculate and update shape info
        var shapeInfo = CalculateShapeInfo(start, end);
        UpdateShapeInfo(shapeInfo);

        // Create preview using strategy pattern
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

        // Use strategy pattern to create final shape
        var strategy = DrawModeStrategyFactory.CreateStrategy(_currentDrawMode);
        var plottable = strategy?.CreateFinal(start, end, _formsPlot.Plot);

        if (plottable != null)
        {
            _formsPlot.Plot.Add.Plottable(plottable);
            _shapeManager.AddShape(plottable);

            // Track for persistence
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

        // Example default styling; in practice, derive from actual plottable styles
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

        // Clear existing shapes
        foreach (var (plottable, _) in _drawnShapes)
            _formsPlot.Plot.Remove(plottable);
        _drawnShapes.Clear();

        // Load and recreate each shape using strategy
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

        var coordinates = _formsPlot.Plot.GetCoordinates(pixelX, pixelY);
        var clickedShape = FindShapeNearPoint(coordinates, pixelX, pixelY);

        bool isCtrlPressed = modifiers.HasFlag(Keys.Control);

        if (clickedShape != null)
        {
            // If Ctrl is not pressed, deselect all other shapes
            if (!isCtrlPressed)
            {
                foreach (var shape in _shapeManager.Shapes)
                    if (shape != clickedShape)
                        shape.IsSelected = false;
            }

            // Toggle selection of clicked shape
            clickedShape.IsSelected = !clickedShape.IsSelected;
        }
        else if (!isCtrlPressed)
        {
            // Clicked on empty area without Ctrl - deselect all
            foreach (var shape in _shapeManager.Shapes)
                shape.IsSelected = false;
        }

        // Update visual appearance and refresh
        UpdateShapeVisuals();
        _formsPlot.Refresh();
    }

    private DrawnShape? FindShapeNearPoint(Coordinates coordinates, int pixelX, int pixelY)
    {
        if (_formsPlot == null)
            return null;

        const double SELECTION_TOLERANCE = 10.0; // pixels

        // Check shapes in reverse order (most recently added first)
        for (int i = _shapeManager.Shapes.Count - 1; i >= 0; i--)
        {
            var shape = _shapeManager.Shapes[i];
            if (!shape.IsVisible)
                continue;

            // Simple bounding-based selection
            if (IsPointNearPlottable(shape.Plottable, coordinates, SELECTION_TOLERANCE))
                return shape;
        }

        return null;
    }

    private bool IsPointNearPlottable(IPlottable plottable, Coordinates coordinates, double tolerance)
    {
        try
        {
            var bounds = plottable.GetAxisLimits();

            // Expand bounds slightly for easier selection
            double margin = (bounds.Rect.Width + bounds.Rect.Height) * 0.02; // 2% margin

            return coordinates.X >= bounds.Rect.Left - margin - tolerance &&
                   coordinates.X <= bounds.Rect.Right + margin + tolerance &&
                   coordinates.Y >= bounds.Rect.Bottom - margin - tolerance &&
                   coordinates.Y <= bounds.Rect.Top + margin + tolerance;
        }
        catch
        {
            return false;
        }
    }

    private void UpdateShapeVisuals()
    {
        // Optionally adjust visuals for selected shapes (e.g., line width)
        // Left intentionally minimal to avoid dependency on specific plottable interfaces
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
        // Placeholder: implement deletion via ShapeManager command(s) if available
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