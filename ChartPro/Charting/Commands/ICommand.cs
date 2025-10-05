namespace ChartPro.Charting.Commands;

/// <summary>
/// Command interface for implementing the Command pattern for undo/redo operations.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    void Execute();

    /// <summary>
    /// Undoes the command.
    /// </summary>
    void Undo();

    /// <summary>
    /// Gets a description of the command for debugging/logging.
    /// </summary>
    string Description { get; }
}
