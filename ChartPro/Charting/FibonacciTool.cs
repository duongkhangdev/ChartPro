using ScottPlot;
using ScottPlot.Plottables;

namespace ChartPro.Charting;

/// <summary>
/// A plottable that renders Fibonacci retracement or extension levels with labels.
/// </summary>
public class FibonacciTool : IPlottable
{
    public bool IsVisible { get; set; } = true;
    public IAxes Axes { get; set; } = new Axes();

    private readonly Coordinates _start;
    private readonly Coordinates _end;
    private readonly List<FibonacciLevel> _levels;
    private readonly bool _isPreview;
    private readonly List<HorizontalLine> _lines = new();
    private readonly List<Text> _labels = new();

    public FibonacciTool(Coordinates start, Coordinates end, List<FibonacciLevel> levels, bool isPreview = false)
    {
        _start = start;
        _end = end;
        _levels = levels;
        _isPreview = isPreview;

        CreateLevels();
    }

    private void CreateLevels()
    {
        double priceRange = _end.Y - _start.Y;
        double minX = Math.Min(_start.X, _end.X);
        double maxX = Math.Max(_start.X, _end.X);

        foreach (var level in _levels.Where(l => l.IsVisible))
        {
            // Calculate price at this Fibonacci level
            double price = _start.Y + (priceRange * level.Ratio);

            // Create horizontal line
            var line = new HorizontalLine
            {
                Y = price,
                LineWidth = _isPreview ? 1 : 2,
                LineColor = _isPreview ? Colors.Gray.WithAlpha(0.5) : level.Color,
                LinePattern = LinePattern.Solid
            };
            _lines.Add(line);

            // Create label showing level and price
            if (!_isPreview)
            {
                var label = new Text
                {
                    LabelText = $"{level.Label} ({price:F2})",
                    Location = new Coordinates(maxX, price),
                    LabelFontSize = 10,
                    LabelFontColor = level.Color,
                    LabelBackgroundColor = Colors.White.WithAlpha(0.7),
                    LabelBorderColor = level.Color,
                    LabelBorderWidth = 1,
                    LabelPadding = 3,
                    LabelAlignment = Alignment.MiddleLeft
                };
                _labels.Add(label);
            }
        }
    }

    public AxisLimits GetAxisLimits()
    {
        double minY = Math.Min(_start.Y, _end.Y);
        double maxY = Math.Max(_start.Y, _end.Y);
        double minX = Math.Min(_start.X, _end.X);
        double maxX = Math.Max(_start.X, _end.X);

        // Expand to include extension levels if any
        foreach (var level in _levels.Where(l => l.IsVisible))
        {
            double price = _start.Y + ((_end.Y - _start.Y) * level.Ratio);
            minY = Math.Min(minY, price);
            maxY = Math.Max(maxY, price);
        }

        return new AxisLimits(minX, maxX, minY, maxY);
    }

    public void Render(RenderPack rp)
    {
        if (!IsVisible)
            return;

        // Render all lines
        foreach (var line in _lines)
        {
            line.Axes = Axes;
            line.Render(rp);
        }

        // Render all labels
        foreach (var label in _labels)
        {
            label.Axes = Axes;
            label.Render(rp);
        }
    }

    public IEnumerable<LegendItem> LegendItems => Enumerable.Empty<LegendItem>();
}
