using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

/// <summary>
/// Strategy for drawing rectangles.
/// </summary>
public class RectangleStrategy : IDrawModeStrategy
{
    public IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot)
    {
        var rect = plot.Add.Rectangle(
            Math.Min(start.X, end.X),
            Math.Max(start.X, end.X),
            Math.Min(start.Y, end.Y),
            Math.Max(start.Y, end.Y));
        rect.LineWidth = 1;
        rect.LineColor = Colors.Gray.WithAlpha(0.5);
        rect.FillColor = Colors.Gray.WithAlpha(0.1);
        return rect;
    }

    public IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot)
    {
        var rect = plot.Add.Rectangle(
            Math.Min(start.X, end.X),
            Math.Max(start.X, end.X),
            Math.Min(start.Y, end.Y),
            Math.Max(start.Y, end.Y));
        rect.LineWidth = 2;
        rect.LineColor = Colors.Purple;
        rect.FillColor = Colors.Purple.WithAlpha(0.1);
        return rect;
    }
}
