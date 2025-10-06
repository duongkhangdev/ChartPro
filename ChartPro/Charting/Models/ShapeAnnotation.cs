namespace ChartPro.Charting.Models;

/// <summary>
/// Represents a serializable shape annotation with all necessary data for persistence.
/// </summary>
public class ShapeAnnotation
{
    /// <summary>
    /// The type of shape (TrendLine, Rectangle, Circle, etc.)
    /// </summary>
    public string ShapeType { get; set; } = string.Empty;

    /// <summary>
    /// Start X coordinate
    /// </summary>
    public double X1 { get; set; }

    /// <summary>
    /// Start Y coordinate
    /// </summary>
    public double Y1 { get; set; }

    /// <summary>
    /// End X coordinate
    /// </summary>
    public double X2 { get; set; }

    /// <summary>
    /// End Y coordinate
    /// </summary>
    public double Y2 { get; set; }

    /// <summary>
    /// Line color in hex format (e.g., "#0000FF")
    /// </summary>
    public string LineColor { get; set; } = "#000000";

    /// <summary>
    /// Line width in pixels
    /// </summary>
    public float LineWidth { get; set; } = 2;

    /// <summary>
    /// Fill color in hex format (e.g., "#FF0000")
    /// </summary>
    public string? FillColor { get; set; }

    /// <summary>
    /// Alpha transparency value (0-255)
    /// </summary>
    public byte FillAlpha { get; set; } = 25;
}

/// <summary>
/// Container for all shape annotations in a chart.
/// </summary>
public class ChartAnnotations
{
    /// <summary>
    /// Version of the annotations format for future compatibility
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// List of all shape annotations
    /// </summary>
    public List<ShapeAnnotation> Shapes { get; set; } = new();
}
