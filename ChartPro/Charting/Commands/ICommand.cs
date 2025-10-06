namespace ChartPro.Charting.Commands;

/// <summary>
/// Interface for the Command pattern to support undo/redo operations.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    void Execute();

    /// <summary>
    /// Undoes the command, reverting to previous state.
    /// </summary>
    void Undo();
}
