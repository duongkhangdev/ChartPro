using ChartPro.Charting.Interactions.Strategies;
using ScottPlot;
using Xunit;

namespace ChartPro.Tests.Strategies;

public class RectangleStrategyTests
{
    private readonly RectangleStrategy _strategy;
    private readonly Plot _plot;

    public RectangleStrategyTests()
    {
        _strategy = new RectangleStrategy();
        _plot = new Plot();
    }

    [Fact]
    public void CreatePreview_ShouldReturnNotNull()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void CreateFinal_ShouldReturnNotNull()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreateFinal(start, end, _plot);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void CreatePreview_ShouldHandleVariousCoordinates()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert - Just verify we get a plottable back
        Assert.NotNull(result);
    }

    [Fact]
    public void CreatePreview_ShouldHandleReversedCoordinates()
    {
        // Arrange - end is "before" start
        var start = new Coordinates(30, 40);
        var end = new Coordinates(10, 20);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert - Just verify we get a plottable back
        Assert.NotNull(result);
    }
}
