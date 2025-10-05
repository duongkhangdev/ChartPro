using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

/// <summary>
/// Strategy for drawing Fibonacci retracement levels.
/// </summary>
public class FibonacciRetracementStrategy : IDrawModeStrategy
{
    public IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot)
    {
        // TODO: Implement full Fibonacci retracement with levels (0.0, 0.236, 0.382, 0.5, 0.618, 0.786, 1.0)
        // For now, create a simple line as preview
        var line = plot.Add.Line(start, end);
        line.LineWidth = 1;
        line.LineColor = Colors.Gold.WithAlpha(0.5);
        return line;
    }

    public IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot)
    {
        // TODO: Implement full Fibonacci retracement with levels and labels
        // For now, create a simple line
        var line = plot.Add.Line(start, end);
        line.LineWidth = 2;
        line.LineColor = Colors.Gold;
        return line;
    }
}
