using ChartPro.Charting;
using ChartPro.Charting.Interactions.Strategies;
using Xunit;

namespace ChartPro.Tests.Strategies;

public class DrawModeStrategyFactoryTests
{
    [Fact]
    public void CreateStrategy_WithTrendLine_ReturnsTrendLineStrategy()
    {
        // Act
        var strategy = DrawModeStrategyFactory.CreateStrategy(ChartDrawMode.TrendLine);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<TrendLineStrategy>(strategy);
    }

    [Fact]
    public void CreateStrategy_WithHorizontalLine_ReturnsHorizontalLineStrategy()
    {
        // Act
        var strategy = DrawModeStrategyFactory.CreateStrategy(ChartDrawMode.HorizontalLine);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<HorizontalLineStrategy>(strategy);
    }

    [Fact]
    public void CreateStrategy_WithVerticalLine_ReturnsVerticalLineStrategy()
    {
        // Act
        var strategy = DrawModeStrategyFactory.CreateStrategy(ChartDrawMode.VerticalLine);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<VerticalLineStrategy>(strategy);
    }

    [Fact]
    public void CreateStrategy_WithRectangle_ReturnsRectangleStrategy()
    {
        // Act
        var strategy = DrawModeStrategyFactory.CreateStrategy(ChartDrawMode.Rectangle);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<RectangleStrategy>(strategy);
    }

    [Fact]
    public void CreateStrategy_WithCircle_ReturnsCircleStrategy()
    {
        // Act
        var strategy = DrawModeStrategyFactory.CreateStrategy(ChartDrawMode.Circle);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<CircleStrategy>(strategy);
    }

    [Fact]
    public void CreateStrategy_WithFibonacciRetracement_ReturnsFibonacciRetracementStrategy()
    {
        // Act
        var strategy = DrawModeStrategyFactory.CreateStrategy(ChartDrawMode.FibonacciRetracement);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<FibonacciRetracementStrategy>(strategy);
    }

    [Fact]
    public void CreateStrategy_WithNone_ReturnsNull()
    {
        // Act
        var strategy = DrawModeStrategyFactory.CreateStrategy(ChartDrawMode.None);

        // Assert
        Assert.Null(strategy);
    }

    [Fact]
    public void CreateStrategy_WithUnimplementedMode_ReturnsNull()
    {
        // Act - FibonacciExtension is not yet implemented
        var strategy = DrawModeStrategyFactory.CreateStrategy(ChartDrawMode.FibonacciExtension);

        // Assert
        Assert.Null(strategy);
    }
}
