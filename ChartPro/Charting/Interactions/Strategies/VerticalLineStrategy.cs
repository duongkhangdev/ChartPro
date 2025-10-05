using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

/// <summary>
/// Strategy for drawing vertical lines.
/// </summary>
public class VerticalLineStrategy : IDrawModeStrategy
{
    public IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot)
    {
        var vLine = plot.Add.VerticalLine(end.X);
        vLine.LineWidth = 1;
        vLine.LineColor = Colors.Gray.WithAlpha(0.5);
        return vLine;
    }

    public IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot)
    {
        var vLine = plot.Add.VerticalLine(end.X);
        vLine.LineWidth = 2;
        vLine.LineColor = Colors.Orange;
        return vLine;
    }
}
