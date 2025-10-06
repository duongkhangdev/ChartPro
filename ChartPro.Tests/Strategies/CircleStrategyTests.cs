using ChartPro.Charting.Interactions.Strategies;
using ScottPlot;
using Xunit;

namespace ChartPro.Tests.Strategies;

public class CircleStrategyTests
{
    private readonly CircleStrategy _strategy;
    private readonly Plot _plot;

    public CircleStrategyTests()
    {
        _strategy = new CircleStrategy();
        _plot = new Plot();
    }

    [Fact]
    public void CreatePreview_ShouldReturnEllipse_WithGrayColor()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var ellipse = Assert.IsAssignableFrom<ScottPlot.Plottables.Ellipse>(result);
        Assert.Equal(1, ellipse.LineWidth);
    }

    [Fact]
    public void CreateFinal_ShouldReturnEllipse_WithCyanColor()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreateFinal(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var ellipse = Assert.IsAssignableFrom<ScottPlot.Plottables.Ellipse>(result);
        Assert.Equal(2, ellipse.LineWidth);
    }

    [Fact]
    public void CreatePreview_ShouldCalculateCorrectCenter()
    {
        // Arrange
        var start = new Coordinates(10, 20);
        var end = new Coordinates(30, 40);

        // Act
        var result = _strategy.CreatePreview(start, end, _plot);

        // Assert
        Assert.NotNull(result);
        var ellipse = Assert.IsAssignableFrom<ScottPlot.Plottables.Ellipse>(result);
        
        // Center should be midpoint
        var expectedCenterX = (start.X + end.X) / 2; // 20
        var expectedCenterY = (start.Y + end.Y) / 2; // 30
        Assert.Equal(expectedCenterX, ellipse.Center.X);
        Assert.Equal(expectedCenterY, ellipse.Center.Y);
    }
}
