namespace ChartPro.Charting;

/// <summary>
/// Represents a Fibonacci level with its ratio, color, and visibility.
/// </summary>
public class FibonacciLevel
{
    public double Ratio { get; set; }
    public string Label { get; set; }
    public ScottPlot.Color Color { get; set; }
    public bool IsVisible { get; set; }

    public FibonacciLevel(double ratio, string label, ScottPlot.Color color, bool isVisible = true)
    {
        Ratio = ratio;
        Label = label;
        Color = color;
        IsVisible = isVisible;
    }

    /// <summary>
    /// Gets the default Fibonacci retracement levels.
    /// </summary>
    public static List<FibonacciLevel> GetDefaultRetracementLevels()
    {
        return new List<FibonacciLevel>
        {
            new FibonacciLevel(0.0, "0.0", ScottPlot.Colors.Red),
            new FibonacciLevel(0.236, "0.236", ScottPlot.Colors.Orange),
            new FibonacciLevel(0.382, "0.382", ScottPlot.Colors.Yellow),
            new FibonacciLevel(0.5, "0.5", ScottPlot.Colors.Green),
            new FibonacciLevel(0.618, "0.618", ScottPlot.Colors.Blue),
            new FibonacciLevel(0.786, "0.786", ScottPlot.Colors.Purple),
            new FibonacciLevel(1.0, "1.0", ScottPlot.Colors.Red)
        };
    }

    /// <summary>
    /// Gets the default Fibonacci extension levels (includes retracement + extension).
    /// </summary>
    public static List<FibonacciLevel> GetDefaultExtensionLevels()
    {
        var levels = GetDefaultRetracementLevels();
        levels.AddRange(new List<FibonacciLevel>
        {
            new FibonacciLevel(1.272, "1.272", ScottPlot.Colors.Cyan),
            new FibonacciLevel(1.618, "1.618", ScottPlot.Colors.Magenta),
            new FibonacciLevel(2.0, "2.0", ScottPlot.Colors.DarkRed),
            new FibonacciLevel(2.618, "2.618", ScottPlot.Colors.DarkBlue)
        });
        return levels;
    }
}
