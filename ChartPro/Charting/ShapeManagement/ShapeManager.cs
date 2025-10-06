using ScottPlot;
using ScottPlot.WinForms;
using ChartPro.Charting.Commands;

namespace ChartPro.Charting.ShapeManagement;

/// <summary>
/// Manages shapes on the chart with undo/redo support using the Command pattern.
/// </summary>
public class ShapeManager : IShapeManager
{
    private FormsPlot? _formsPlot;
    private readonly List<IPlottable> _shapes = new();
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();
    private bool _isAttached;
    private bool _disposed;

    public IReadOnlyList<IPlottable> Shapes => _shapes.AsReadOnly();
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
    public void AddShape(IPlottable shape)
    {
        if (_formsPlot == null)
        {
            throw new InvalidOperationException("ShapeManager is not attached to a FormsPlot control.");
        }

        if (shape == null)
        {
            throw new ArgumentNullException(nameof(shape));
        }

        var command = new AddShapeCommand(_formsPlot, shape);
        ExecuteCommand(command);
        _shapes.Add(shape);
    }

    /// <summary>
    /// Deletes a shape from the chart using the Command pattern.
    /// </summary>
    public void DeleteShape(IPlottable shape)
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

        var command = new DeleteShapeCommand(_formsPlot, shape);
        ExecuteCommand(command);
        _shapes.Remove(shape);
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
            _shapes.Remove(addCmd.Shape);
        }
        else if (command is DeleteShapeCommand delCmd)
        {
            _shapes.Add(delCmd.Shape);
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
            _shapes.Add(addCmd.Shape);
        }
        else if (command is DeleteShapeCommand delCmd)
        {
            _shapes.Remove(delCmd.Shape);
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
            _undoStack.Clear();
            _redoStack.Clear();
        }

        _isAttached = false;
        _disposed = true;
    }

    #endregion
}
