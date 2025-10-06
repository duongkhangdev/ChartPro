using ChartPro.Charting.Commands;
using ChartPro.Charting.Shapes;
using ScottPlot;
using ScottPlot.WinForms;
using System.Drawing;
using ChartPro.Charting.ShapeManagement;

namespace ChartPro.Charting.Interactions;

/// <summary>
/// DI-based service for managing chart interactions, drawing tools, and real-time updates.
/// Handles mouse events, shape drawing, Fibonacci tools, channels, and live candle updates.
/// </summary>
public class ChartInteractions : IChartInteractions
{
    private readonly IShapeManager _shapeManager;
    private FormsPlot? _formsPlot;
    private int _pricePlotIndex;
    private ChartDrawMode _currentDrawMode = ChartDrawMode.None;
    private List<OHLC>? _boundCandles;
    private bool _isAttached;
    private bool _disposed;

    // Drawing state
    private Coordinates? _drawStartCoordinates;
    private IPlottable? _previewPlottable;
    private Coordinates? _currentMouseCoordinates;
    private string? _currentShapeInfo;

    // Shape management
    private readonly IShapeManager _shapeManager;

    public ChartDrawMode CurrentDrawMode => _currentDrawMode;
    public bool IsAttached => _isAttached;
    public IShapeManager ShapeManager => _shapeManager;

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
        {
            throw new InvalidOperationException("Already attached to a FormsPlot control. Call Dispose first.");
        }

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

        // Clear shape info when changing modes
        UpdateShapeInfo(null);

        // Disable pan/zoom when in drawing mode
        if (mode != ChartDrawMode.None && _formsPlot != null)
        {
            _formsPlot.UserInputProcessor.IsEnabled = false;
        }
        else if (_formsPlot != null)
        {
            _formsPlot.UserInputProcessor.IsEnabled = true;
        }

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
        if (_formsPlot == null)
            return;

        if (_currentDrawMode == ChartDrawMode.None)
        {
            // Selection mode - try to select a shape
            if (e.Button == MouseButtons.Left)
            {
                HandleShapeSelection(e.X, e.Y, Control.ModifierKeys);
            }
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
            _ => string.Empty
        };
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
            _shapeManager.AddShape(plottable);
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
                {
                    if (shape != clickedShape)
                    {
                        shape.IsSelected = false;
                    }
                }
            }

            // Toggle selection of clicked shape
            clickedShape.IsSelected = !clickedShape.IsSelected;
        }
        else if (!isCtrlPressed)
        {
            // Clicked on empty area without Ctrl - deselect all
            foreach (var shape in _shapeManager.Shapes)
            {
                shape.IsSelected = false;
            }
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

            // Simple distance-based selection
            // For more complex shapes, this could be improved with actual geometry tests
            if (IsPointNearPlottable(shape.Plottable, coordinates, pixelX, pixelY, SELECTION_TOLERANCE))
            {
                return shape;
            }
        }

        return null;
    }

    private bool IsPointNearPlottable(IPlottable plottable, Coordinates coordinates, int pixelX, int pixelY, double tolerance)
    {
        // This is a simplified implementation
        // In a production system, you'd want more sophisticated hit testing based on plottable type
        
        // For now, use the plottable's axis limits as a rough bounding box
        try
        {
            var bounds = plottable.GetAxisLimits();
            
            // Expand bounds slightly for easier selection
            double margin = (bounds.Rect.Width + bounds.Rect.Height) * 0.02; // 2% margin
            
            return coordinates.X >= bounds.Rect.Left - margin &&
                   coordinates.X <= bounds.Rect.Right + margin &&
                   coordinates.Y >= bounds.Rect.Bottom - margin &&
                   coordinates.Y <= bounds.Rect.Top + margin;
        }
        catch
        {
            // If we can't get bounds, skip this plottable
            return false;
        }
    }

    private void UpdateShapeVisuals()
    {
        foreach (var shape in _shapeManager.Shapes)
        {
            // Update visual appearance based on selection state
            // This is a basic implementation - you could enhance it with different colors, line widths, etc.
            try
            {
                // Try to access common line properties
                if (shape.Plottable is IHasLine linePlottable)
                {
                    // Make selected shapes more prominent
                    linePlottable.LineWidth = shape.IsSelected ? 3 : 2;
                }
            }
            catch
            {
                // Some plottables might not support these properties
            }
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
        if (_formsPlot == null)
            return;

        var selectedShapes = _shapeManager.Shapes.Where(s => s.IsSelected).ToList();
        
        foreach (var shape in selectedShapes)
        {
            var command = new DeleteShapeCommand(_shapeManager, shape, _formsPlot.Plot);
            _shapeManager.ExecuteCommand(command);
        }

        if (selectedShapes.Any())
        {
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
