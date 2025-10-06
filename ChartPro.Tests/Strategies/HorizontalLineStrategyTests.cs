using ChartPro.Charting.Interactions.Strategies;
using ScottPlot;
using Xunit;

namespace ChartPro.Tests.Strategies;

public class HorizontalLineStrategyTests
{
    private readonly HorizontalLineStrategy _strategy;
    private readonly Plot _plot;

    public HorizontalLineStrategyTests()
    {
        _strategy = new HorizontalLineStrategy();
        _plot = new Plot();
    }

    [Fact]
    public void CreatePreview_ShouldReturnHorizontalLine_WithGrayColor()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var hLine = Assert.IsAssignableFrom<ScottPlot.Plottables.HorizontalLine>(result);
        Assert.Equal(1, hLine.LineWidth);
    }

    [Fact]
    public void CreateFinal_ShouldReturnHorizontalLine_WithGreenColor()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreateFinal(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var hLine = Assert.IsAssignableFrom<ScottPlot.Plottables.HorizontalLine>(result);
        Assert.Equal(2, hLine.LineWidth);
    }

    [Fact]
    public void CreatePreview_ShouldUseEndYCoordinate()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var hLine = Assert.IsAssignableFrom<ScottPlot.Plottables.HorizontalLine>(result);
        Assert.Equal(end.Y, hLine.Y);
    }
}
