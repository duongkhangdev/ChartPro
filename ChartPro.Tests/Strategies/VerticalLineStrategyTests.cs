using ChartPro.Charting.Interactions.Strategies;
using ScottPlot;
using Xunit;

namespace ChartPro.Tests.Strategies;

public class VerticalLineStrategyTests
{
    private readonly VerticalLineStrategy _strategy;
    private readonly Plot _plot;

    public VerticalLineStrategyTests()
    {
        _strategy = new VerticalLineStrategy();
        _plot = new Plot();
    }

    [Fact]
    public void CreatePreview_ShouldReturnVerticalLine_WithGrayColor()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var vLine = Assert.IsAssignableFrom<ScottPlot.Plottables.VerticalLine>(result);
        Assert.Equal(1, vLine.LineWidth);
    }

    [Fact]
    public void CreateFinal_ShouldReturnVerticalLine_WithOrangeColor()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreateFinal(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var vLine = Assert.IsAssignableFrom<ScottPlot.Plottables.VerticalLine>(result);
        Assert.Equal(2, vLine.LineWidth);
    }

    [Fact]
    public void CreatePreview_ShouldUseEndXCoordinate()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var vLine = Assert.IsAssignableFrom<ScottPlot.Plottables.VerticalLine>(result);
        Assert.Equal(end.X, vLine.X);
    }
}
