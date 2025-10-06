namespace ChartPro.Charting.Interactions.Strategies;

/// <summary>
/// Factory for creating draw mode strategy instances.
/// </summary>
public class DrawModeStrategyFactory
{
    /// <summary>
    /// Creates a strategy instance for the specified draw mode.
    /// </summary>
    /// <param name="mode">The draw mode</param>
    /// <returns>Strategy instance, or null if mode is None or not implemented</returns>
    public static IDrawModeStrategy? CreateStrategy(ChartDrawMode mode)
    {
        return mode switch
        {
            ChartDrawMode.TrendLine => new TrendLineStrategy(),
            ChartDrawMode.HorizontalLine => new HorizontalLineStrategy(),
            ChartDrawMode.VerticalLine => new VerticalLineStrategy(),
            ChartDrawMode.Rectangle => new RectangleStrategy(),
            ChartDrawMode.Circle => new CircleStrategy(),
            ChartDrawMode.FibonacciRetracement => new FibonacciRetracementStrategy(),
            // TODO: Implement strategies for FibonacciExtension, Channel, Triangle, Text
            _ => null
        };
    }
}
