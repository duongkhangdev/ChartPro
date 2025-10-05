using ChartPro.Charting.Commands;

namespace ChartPro.Charting.Shapes;

/// <summary>
/// Manages drawn shapes and their state, including undo/redo functionality.
/// </summary>
public class ShapeManager : IShapeManager
{
    private readonly List<DrawnShape> _shapes = new();
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();

    public IReadOnlyList<DrawnShape> Shapes => _shapes.AsReadOnly();

    public bool CanUndo => _undoStack.Count > 0;

    public bool CanRedo => _redoStack.Count > 0;

    public void ExecuteCommand(ICommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear(); // Clear redo stack when new command is executed
    }

    public bool Undo()
    {
        if (!CanUndo)
            return false;

        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        return true;
    }

    public bool Redo()
    {
        if (!CanRedo)
            return false;

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        return true;
    }

    public void Clear()
    {
        _shapes.Clear();
        _undoStack.Clear();
        _redoStack.Clear();
    }

    public DrawnShape? GetShapeById(Guid id)
    {
        return _shapes.FirstOrDefault(s => s.Id == id);
    }

    public void RemoveShape(DrawnShape shape)
    {
        if (shape == null)
            throw new ArgumentNullException(nameof(shape));

        _shapes.Remove(shape);
    }

    public void AddShape(DrawnShape shape)
    {
        if (shape == null)
            throw new ArgumentNullException(nameof(shape));

        _shapes.Add(shape);
    }
}
