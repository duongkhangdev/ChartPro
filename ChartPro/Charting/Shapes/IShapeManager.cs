using ChartPro.Charting.Commands;
using ScottPlot;

namespace ChartPro.Charting.Shapes;

/// <summary>
/// Interface for managing drawn shapes and their state.
/// </summary>
public interface IShapeManager
{
    /// <summary>
    /// Gets all shapes managed by this manager.
    /// </summary>
    IReadOnlyList<DrawnShape> Shapes { get; }

    /// <summary>
    /// Executes a command and adds it to the undo stack.
    /// </summary>
    void ExecuteCommand(ICommand command);

    /// <summary>
    /// Undoes the last command if available.
    /// </summary>
    /// <returns>True if undo was performed, false otherwise.</returns>
    bool Undo();

    /// <summary>
    /// Redoes the last undone command if available.
    /// </summary>
    /// <returns>True if redo was performed, false otherwise.</returns>
    bool Redo();

    /// <summary>
    /// Gets whether undo is available.
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// Gets whether redo is available.
    /// </summary>
    bool CanRedo { get; }

    /// <summary>
    /// Clears all shapes and command history.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets a shape by its ID.
    /// </summary>
    DrawnShape? GetShapeById(Guid id);

    /// <summary>
    /// Removes a shape from the manager.
    /// </summary>
    void RemoveShape(DrawnShape shape);

    /// <summary>
    /// Adds a shape to the manager.
    /// </summary>
    void AddShape(DrawnShape shape);
}
