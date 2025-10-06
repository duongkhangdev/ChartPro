using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

/// <summary>
/// Strategy for drawing horizontal lines.
/// </summary>
public class HorizontalLineStrategy : IDrawModeStrategy
{
    public IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot)
    {
        var hLine = plot.Add.HorizontalLine(end.Y);
        hLine.LineWidth = 1;
        hLine.LineColor = Colors.Gray.WithAlpha(0.5);
        return hLine;
    }

    public IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot)
    {
        var hLine = plot.Add.HorizontalLine(end.Y);
        hLine.LineWidth = 2;
        hLine.LineColor = Colors.Green;
        return hLine;
    }
}
