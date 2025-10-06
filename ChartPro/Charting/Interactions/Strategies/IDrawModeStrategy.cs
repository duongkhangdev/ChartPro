using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

/// <summary>
/// Interface for draw mode strategy implementations.
/// Each draw mode (TrendLine, Rectangle, etc.) implements this interface.
/// </summary>
public interface IDrawModeStrategy
{
    /// <summary>
    /// Creates a preview plottable for the current drawing operation.
    /// </summary>
    /// <param name="start">Starting coordinates</param>
    /// <param name="end">Current coordinates</param>
    /// <param name="plot">The plot to add the preview to</param>
    /// <returns>Preview plottable to be displayed while drawing</returns>
    IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot);

    /// <summary>
    /// Creates the final plottable after drawing is complete.
    /// </summary>
    /// <param name="start">Starting coordinates</param>
    /// <param name="end">Ending coordinates</param>
    /// <param name="plot">The plot to add the final shape to</param>
    /// <returns>Final plottable to be added to the chart</returns>
    IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot);
}
