using ScottPlot;
using ScottPlot.WinForms;

namespace ChartPro.Charting.ShapeManagement;

/// <summary>
/// Interface for managing shapes on the chart with undo/redo support.
/// </summary>
public interface IShapeManager : IDisposable
{
    /// <summary>
    /// Attaches the shape manager to a FormsPlot control.
    /// </summary>
    /// <param name="formsPlot">The FormsPlot control to attach to</param>
    void Attach(FormsPlot formsPlot);

    /// <summary>
    /// Adds a shape to the chart.
    /// </summary>
    /// <param name="shape">The shape to add</param>
    void AddShape(IPlottable shape);

    /// <summary>
    /// Deletes a shape from the chart.
    /// </summary>
    /// <param name="shape">The shape to delete</param>
    void DeleteShape(IPlottable shape);

    /// <summary>
    /// Undoes the last command.
    /// </summary>
    /// <returns>True if undo was successful, false if there's nothing to undo</returns>
    bool Undo();

    /// <summary>
    /// Redoes the last undone command.
    /// </summary>
    /// <returns>True if redo was successful, false if there's nothing to redo</returns>
    bool Redo();

    /// <summary>
    /// Gets all shapes currently managed.
    /// </summary>
    IReadOnlyList<IPlottable> Shapes { get; }

    /// <summary>
    /// Gets whether there are commands that can be undone.
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// Gets whether there are commands that can be redone.
    /// </summary>
    bool CanRedo { get; }

    /// <summary>
    /// Gets whether the manager is attached to a chart.
    /// </summary>
    bool IsAttached { get; }
}
