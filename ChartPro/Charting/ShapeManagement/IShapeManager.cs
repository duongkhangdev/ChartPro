using ScottPlot;
using ScottPlot.WinForms;
using ChartPro.Charting.Shapes;

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
    /// <param name="shape">The DrawnShape to add</param>
    void AddShape(DrawnShape shape);

    /// <summary>
    /// Deletes a shape from the chart.
    /// </summary>
    /// <param name="shape">The DrawnShape to delete</param>
    void DeleteShape(DrawnShape shape);

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
    IReadOnlyList<DrawnShape> Shapes { get; }

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

    /// <summary>
    /// Selects a shape at the given pixel coordinates.
    /// </summary>
    /// <param name="pixelX">The X pixel coordinate</param>
    /// <param name="pixelY">The Y pixel coordinate</param>
    /// <param name="addToSelection">If true, adds to existing selection; if false, replaces selection</param>
    /// <returns>The selected shape, or null if no shape was found</returns>
    DrawnShape? SelectShapeAt(int pixelX, int pixelY, bool addToSelection = false);

    /// <summary>
    /// Toggles selection state of a specific shape.
    /// </summary>
    /// <param name="shape">The shape to toggle</param>
    void ToggleSelection(DrawnShape shape);

    /// <summary>
    /// Clears all selections.
    /// </summary>
    void ClearSelection();

    /// <summary>
    /// Gets all currently selected shapes.
    /// </summary>
    IReadOnlyList<DrawnShape> SelectedShapes { get; }

    /// <summary>
    /// Deletes all selected shapes.
    /// </summary>
    void DeleteSelectedShapes();
}
