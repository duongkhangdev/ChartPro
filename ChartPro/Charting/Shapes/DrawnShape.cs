using ScottPlot;

namespace ChartPro.Charting.Shapes;

/// <summary>
/// Represents a drawn shape with its associated plottable and metadata.
/// </summary>
public class DrawnShape
{
    /// <summary>
    /// Unique identifier for the shape.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The ScottPlot plottable object.
    /// </summary>
    public IPlottable Plottable { get; }

    /// <summary>
    /// The drawing mode used to create this shape.
    /// </summary>
    public ChartDrawMode DrawMode { get; }

    /// <summary>
    /// Whether the shape is currently selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Whether the shape is visible.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// When the shape was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    public DrawnShape(IPlottable plottable, ChartDrawMode drawMode)
    {
        Id = Guid.NewGuid();
        Plottable = plottable ?? throw new ArgumentNullException(nameof(plottable));
        DrawMode = drawMode;
        IsVisible = true;
        IsSelected = false;
        CreatedAt = DateTime.UtcNow;
    }
}
