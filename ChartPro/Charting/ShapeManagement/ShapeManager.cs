using ScottPlot;
using ScottPlot.WinForms;
using ChartPro.Charting.Commands;
using ChartPro.Charting.Shapes;

namespace ChartPro.Charting.ShapeManagement;

/// <summary>
/// Manages shapes on the chart with undo/redo support using the Command pattern.
/// </summary>
public class ShapeManager : IShapeManager
{
    private FormsPlot? _formsPlot;
    private readonly List<DrawnShape> _shapes = new();
    private readonly Dictionary<IPlottable, DrawnShape> _plottableToShape = new();
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();
    private bool _isAttached;
    private bool _disposed;

    public IReadOnlyList<DrawnShape> Shapes => _shapes.AsReadOnly();
    public IReadOnlyList<DrawnShape> SelectedShapes => _shapes.Where(s => s.IsSelected).ToList().AsReadOnly();
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public bool IsAttached => _isAttached;

    /// <summary>
    /// Attaches the shape manager to a FormsPlot control.
    /// </summary>
    public void Attach(FormsPlot formsPlot)
    {
        if (_isAttached)
        {
            throw new InvalidOperationException("Already attached to a FormsPlot control. Call Dispose first.");
        }

        _formsPlot = formsPlot ?? throw new ArgumentNullException(nameof(formsPlot));
        _isAttached = true;
    }

    /// <summary>
    /// Adds a shape to the chart using the Command pattern.
    /// </summary>
    public void AddShape(DrawnShape shape)
    {
        if (_formsPlot == null)
        {
            throw new InvalidOperationException("ShapeManager is not attached to a FormsPlot control.");
        }

        if (shape == null)
        {
            throw new ArgumentNullException(nameof(shape));
        }

        var command = new AddShapeCommand(_formsPlot, shape.Plottable);
        ExecuteCommand(command);
        _shapes.Add(shape);
        _plottableToShape[shape.Plottable] = shape;
    }

    /// <summary>
    /// Deletes a shape from the chart using the Command pattern.
    /// </summary>
    public void DeleteShape(DrawnShape shape)
    {
        if (_formsPlot == null)
        {
            throw new InvalidOperationException("ShapeManager is not attached to a FormsPlot control.");
        }

        if (shape == null)
        {
            throw new ArgumentNullException(nameof(shape));
        }

        if (!_shapes.Contains(shape))
        {
            throw new InvalidOperationException("Shape is not managed by this ShapeManager.");
        }

        var command = new DeleteShapeCommand(_formsPlot, shape.Plottable);
        ExecuteCommand(command);
        _shapes.Remove(shape);
        _plottableToShape.Remove(shape.Plottable);
    }

    /// <summary>
    /// Deletes all selected shapes.
    /// </summary>
    public void DeleteSelectedShapes()
    {
        if (_formsPlot == null)
        {
            throw new InvalidOperationException("ShapeManager is not attached to a FormsPlot control.");
        }

        var selectedShapes = _shapes.Where(s => s.IsSelected).ToList();
        foreach (var shape in selectedShapes)
        {
            DeleteShape(shape);
        }
    }

    /// <summary>
    /// Undoes the last command.
    /// </summary>
    public bool Undo()
    {
        if (!CanUndo)
            return false;

        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);

        // Update shapes list based on command type
        if (command is AddShapeCommand addCmd)
        {
            // Find and remove the DrawnShape corresponding to this IPlottable
            if (_plottableToShape.TryGetValue(addCmd.Shape, out var drawnShape))
            {
                _shapes.Remove(drawnShape);
                _plottableToShape.Remove(addCmd.Shape);
            }
        }
        else if (command is DeleteShapeCommand delCmd)
        {
            // Find and re-add the DrawnShape corresponding to this IPlottable
            if (_plottableToShape.TryGetValue(delCmd.Shape, out var drawnShape))
            {
                _shapes.Add(drawnShape);
            }
        }

        return true;
    }

    /// <summary>
    /// Redoes the last undone command.
    /// </summary>
    public bool Redo()
    {
        if (!CanRedo)
            return false;

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);

        // Update shapes list based on command type
        if (command is AddShapeCommand addCmd)
        {
            // Find and re-add the DrawnShape corresponding to this IPlottable
            if (_plottableToShape.TryGetValue(addCmd.Shape, out var drawnShape))
            {
                _shapes.Add(drawnShape);
            }
        }
        else if (command is DeleteShapeCommand delCmd)
        {
            // Find and remove the DrawnShape corresponding to this IPlottable
            if (_plottableToShape.TryGetValue(delCmd.Shape, out var drawnShape))
            {
                _shapes.Remove(drawnShape);
                _plottableToShape.Remove(delCmd.Shape);
            }
        }

        return true;
    }

    /// <summary>
    /// Executes a command and adds it to the undo stack.
    /// </summary>
    private void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear(); // Clear redo stack when a new command is executed
    }

    /// <summary>
    /// Selects a shape at the given pixel coordinates.
    /// </summary>
    public DrawnShape? SelectShapeAt(int pixelX, int pixelY, bool addToSelection = false)
    {
        if (_formsPlot == null)
            return null;

        // Convert pixel coordinates to data coordinates
        var coords = _formsPlot.Plot.GetCoordinates(pixelX, pixelY);
        
        // Find the shape closest to the click point
        // For now, we'll use a simple distance check
        // In the future, this could be optimized with spatial indexing (R-tree)
        DrawnShape? closestShape = null;
        double minDistance = double.MaxValue;
        const double selectionThreshold = 0.05; // 5% of visible range

        var xRange = _formsPlot.Plot.Axes.GetLimits().Rect.Width;
        var yRange = _formsPlot.Plot.Axes.GetLimits().Rect.Height;
        var threshold = Math.Min(xRange, yRange) * selectionThreshold;

        foreach (var shape in _shapes.Where(s => s.IsVisible))
        {
            var distance = CalculateDistanceToShape(shape, coords);
            if (distance < minDistance && distance < threshold)
            {
                minDistance = distance;
                closestShape = shape;
            }
        }

        // Update selection
        if (!addToSelection)
        {
            // Clear existing selection if not adding to it
            foreach (var shape in _shapes)
            {
                if (shape.IsSelected)
                {
                    shape.IsSelected = false;
                    UpdateShapeVisual(shape);
                }
            }
        }

        if (closestShape != null)
        {
            closestShape.IsSelected = true;
            UpdateShapeVisual(closestShape);
            _formsPlot.Refresh();
        }

        return closestShape;
    }

    /// <summary>
    /// Toggles selection state of a specific shape.
    /// </summary>
    public void ToggleSelection(DrawnShape shape)
    {
        if (shape == null)
            throw new ArgumentNullException(nameof(shape));

        if (!_shapes.Contains(shape))
            throw new InvalidOperationException("Shape is not managed by this ShapeManager.");

        shape.IsSelected = !shape.IsSelected;
        UpdateShapeVisual(shape);
        _formsPlot?.Refresh();
    }

    /// <summary>
    /// Clears all selections.
    /// </summary>
    public void ClearSelection()
    {
        var anyChanged = false;
        foreach (var shape in _shapes)
        {
            if (shape.IsSelected)
            {
                shape.IsSelected = false;
                UpdateShapeVisual(shape);
                anyChanged = true;
            }
        }

        if (anyChanged)
        {
            _formsPlot?.Refresh();
        }
    }

    /// <summary>
    /// Calculates the distance from a point to a shape.
    /// This is a simplified implementation - could be enhanced for different shape types.
    /// </summary>
    private double CalculateDistanceToShape(DrawnShape shape, Coordinates point)
    {
        // For now, calculate distance to the shape's bounding box center
        // This is a simple approximation and could be improved per shape type
        var plottable = shape.Plottable;
        
        // Try to get bounds from the plottable
        // This is a basic implementation - ScottPlot shapes may have different properties
        try
        {
            var bounds = plottable.GetAxisLimits();
            var centerX = (bounds.Left + bounds.Right) / 2;
            var centerY = (bounds.Bottom + bounds.Top) / 2;
            
            var dx = point.X - centerX;
            var dy = point.Y - centerY;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        catch
        {
            // If we can't get bounds, return a large distance
            return double.MaxValue;
        }
    }

    /// <summary>
    /// Updates the visual appearance of a shape based on its selection state.
    /// </summary>
    private void UpdateShapeVisual(DrawnShape shape)
    {
        // Update the visual appearance of the plottable based on selection state
        // For selected shapes, we'll make them more prominent
        var plottable = shape.Plottable;
        
        // This is a simplified implementation
        // Different plottable types have different properties
        // In a production system, you'd want to handle each type specifically
        try
        {
            // Try to access common properties through reflection or type checking
            var plottableType = plottable.GetType();
            
            // Look for LineWidth property
            var lineWidthProp = plottableType.GetProperty("LineWidth");
            if (lineWidthProp != null && lineWidthProp.CanWrite)
            {
                var currentWidth = (float?)lineWidthProp.GetValue(plottable) ?? 2f;
                var newWidth = shape.IsSelected ? currentWidth * 1.5f : Math.Max(2f, currentWidth / 1.5f);
                lineWidthProp.SetValue(plottable, newWidth);
            }

            // Look for LineColor property for selection highlighting
            var lineColorProp = plottableType.GetProperty("LineColor");
            if (lineColorProp != null && lineColorProp.CanWrite && shape.IsSelected)
            {
                // Store original color if needed and change to highlight color
                // For now, we'll just make it brighter/more visible
                lineColorProp.SetValue(plottable, ScottPlot.Colors.Yellow);
            }
        }
        catch
        {
            // If we can't update the visual, that's OK - just skip it
        }
    }

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
            _formsPlot = null;
            _shapes.Clear();
            _plottableToShape.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
        }

        _isAttached = false;
        _disposed = true;
    }

    #endregion
}
