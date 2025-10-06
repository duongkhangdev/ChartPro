using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

/// <summary>
/// Strategy for drawing trend lines.
/// </summary>
public class TrendLineStrategy : IDrawModeStrategy
{
    public IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot)
    {
        var line = plot.Add.Line(start, end);
        line.LineWidth = 1;
        line.LineColor = Colors.Gray.WithAlpha(0.5);
        return line;
    }

    public IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot)
    {
        var line = plot.Add.Line(start, end);
        line.LineWidth = 2;
        line.LineColor = Colors.Blue;
        return line;
    }
}
