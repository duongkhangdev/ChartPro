using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

/// <summary>
/// Strategy for drawing circles (ellipses).
/// </summary>
public class CircleStrategy : IDrawModeStrategy
{
    public IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot)
    {
        var centerX = (start.X + end.X) / 2;
        var centerY = (start.Y + end.Y) / 2;
        var radiusX = Math.Abs(end.X - start.X) / 2;
        var radiusY = Math.Abs(end.Y - start.Y) / 2;
        
        var circle = plot.Add.Ellipse(centerX, centerY, radiusX, radiusY);
        circle.LineWidth = 1;
        circle.LineColor = Colors.Gray.WithAlpha(0.5);
        circle.FillColor = Colors.Gray.WithAlpha(0.1);
        return circle;
    }

    public IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot)
    {
        var centerX = (start.X + end.X) / 2;
        var centerY = (start.Y + end.Y) / 2;
        var radiusX = Math.Abs(end.X - start.X) / 2;
        var radiusY = Math.Abs(end.Y - start.Y) / 2;
        
        var circle = plot.Add.Ellipse(centerX, centerY, radiusX, radiusY);
        circle.LineWidth = 2;
        circle.LineColor = Colors.Cyan;
        circle.FillColor = Colors.Cyan.WithAlpha(0.1);
        return circle;
    }
}
