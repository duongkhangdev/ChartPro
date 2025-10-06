using ChartPro.Charting.Interactions.Strategies;
using ScottPlot;
using Xunit;

namespace ChartPro.Tests.Strategies;

public class TrendLineStrategyTests
{
    private readonly TrendLineStrategy _strategy;
    private readonly Plot _plot;

    public TrendLineStrategyTests()
    {
        _strategy = new TrendLineStrategy();
        _plot = new Plot();
    }

    [Fact]
    public void CreatePreview_ShouldReturnLine_WithGrayColor()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var line = Assert.IsAssignableFrom<ScottPlot.Plottables.LinePlot>(result);
        Assert.Equal(1, line.LineWidth);
    }

    [Fact]
    public void CreateFinal_ShouldReturnLine_WithBlueColor()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreateFinal(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var line = Assert.IsAssignableFrom<ScottPlot.Plottables.LinePlot>(result);
        Assert.Equal(2, line.LineWidth);
    }

    [Fact]
    public void CreatePreview_ShouldUseCorrectCoordinates()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var line = Assert.IsAssignableFrom<ScottPlot.Plottables.LinePlot>(result);
        Assert.Equal(start.X, line.Start.X);
        Assert.Equal(start.Y, line.Start.Y);
        Assert.Equal(end.X, line.End.X);
        Assert.Equal(end.Y, line.End.Y);
    }
}
