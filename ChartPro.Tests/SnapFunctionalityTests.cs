using ChartPro.Charting;
using ChartPro.Charting.Interactions;
using ChartPro.Charting.ShapeManagement;
using ScottPlot;
using ScottPlot.WinForms;

namespace ChartPro.Tests;

public class SnapFunctionalityTests : IDisposable
{
    private readonly FormsPlot _formsPlot;
    private readonly IShapeManager _shapeManager;
    private readonly ChartInteractions _chartInteractions;

    public SnapFunctionalityTests()
    {
        _formsPlot = new FormsPlot();
        _shapeManager = new ShapeManager();
        _chartInteractions = new ChartInteractions(_shapeManager);
        _chartInteractions.Attach(_formsPlot);
    }

    public void Dispose()
    {
        _chartInteractions?.Dispose();
        _shapeManager?.Dispose();
        _formsPlot?.Dispose();
    }

    [Fact]
    public void SnapEnabled_DefaultsToFalse()
    {
        // Assert
        Assert.False(_chartInteractions.SnapEnabled);
    }

    [Fact]
    public void SnapMode_DefaultsToNone()
    {
        // Assert
        Assert.Equal(SnapMode.None, _chartInteractions.SnapMode);
    }

    [Fact]
    public void SnapEnabled_CanBeSet()
    {
        // Act
        _chartInteractions.SnapEnabled = true;

        // Assert
        Assert.True(_chartInteractions.SnapEnabled);
    }

    [Fact]
    public void SnapMode_CanBeSetToPrice()
    {
        // Act
        _chartInteractions.SnapMode = SnapMode.Price;

        // Assert
        Assert.Equal(SnapMode.Price, _chartInteractions.SnapMode);
    }

    [Fact]
    public void SnapMode_CanBeSetToCandleOHLC()
    {
        // Act
        _chartInteractions.SnapMode = SnapMode.CandleOHLC;

        // Assert
        Assert.Equal(SnapMode.CandleOHLC, _chartInteractions.SnapMode);
    }

    [Fact]
    public void BindCandles_AllowsSnapToCandleOHLC()
    {
        // Arrange
        var candles = GenerateSampleCandles(10);
        
        // Act
        _chartInteractions.BindCandles(candles);
        _chartInteractions.SnapEnabled = true;
        _chartInteractions.SnapMode = SnapMode.CandleOHLC;

        // Assert - No exception should be thrown
        Assert.True(_chartInteractions.SnapEnabled);
        Assert.Equal(SnapMode.CandleOHLC, _chartInteractions.SnapMode);
    }

    [Fact]
    public void SnapMode_CanSwitchBetweenModes()
    {
        // Act & Assert
        _chartInteractions.SnapMode = SnapMode.Price;
        Assert.Equal(SnapMode.Price, _chartInteractions.SnapMode);

        _chartInteractions.SnapMode = SnapMode.CandleOHLC;
        Assert.Equal(SnapMode.CandleOHLC, _chartInteractions.SnapMode);

        _chartInteractions.SnapMode = SnapMode.None;
        Assert.Equal(SnapMode.None, _chartInteractions.SnapMode);
    }

    [Fact]
    public void SnapEnabled_IndependentOfSnapMode()
    {
        // Arrange
        _chartInteractions.SnapEnabled = true;

        // Act - Change mode shouldn't affect enabled state
        _chartInteractions.SnapMode = SnapMode.Price;
        var enabledAfterModeChange = _chartInteractions.SnapEnabled;

        // Assert
        Assert.True(enabledAfterModeChange);
    }

    [Fact]
    public void SnapMode_IndependentOfSnapEnabled()
    {
        // Arrange
        _chartInteractions.SnapMode = SnapMode.Price;

        // Act - Change enabled shouldn't affect mode
        _chartInteractions.SnapEnabled = true;
        var modeAfterEnabledChange = _chartInteractions.SnapMode;

        // Assert
        Assert.Equal(SnapMode.Price, modeAfterEnabledChange);
    }

    [Theory]
    [InlineData(SnapMode.None)]
    [InlineData(SnapMode.Price)]
    [InlineData(SnapMode.CandleOHLC)]
    public void SnapMode_AcceptsAllValidValues(SnapMode mode)
    {
        // Act
        _chartInteractions.SnapMode = mode;

        // Assert
        Assert.Equal(mode, _chartInteractions.SnapMode);
    }

    [Fact]
    public void ChartInteractions_CanAttachWithSnap()
    {
        // Arrange
        using var newFormsPlot = new FormsPlot();
        using var newShapeManager = new ShapeManager();
        using var newChartInteractions = new ChartInteractions(newShapeManager);

        // Act
        newChartInteractions.Attach(newFormsPlot);
        newChartInteractions.SnapEnabled = true;
        newChartInteractions.SnapMode = SnapMode.Price;

        // Assert
        Assert.True(newChartInteractions.IsAttached);
        Assert.True(newChartInteractions.SnapEnabled);
        Assert.Equal(SnapMode.Price, newChartInteractions.SnapMode);
    }

    [Fact]
    public void SetDrawMode_WorksWithSnapEnabled()
    {
        // Arrange
        _chartInteractions.SnapEnabled = true;
        _chartInteractions.SnapMode = SnapMode.Price;

        // Act
        _chartInteractions.SetDrawMode(ChartDrawMode.TrendLine);

        // Assert
        Assert.Equal(ChartDrawMode.TrendLine, _chartInteractions.CurrentDrawMode);
        Assert.True(_chartInteractions.SnapEnabled); // Should remain enabled
    }

    [Fact]
    public void BindCandles_WithEmptyList_DoesNotThrow()
    {
        // Arrange
        var emptyCandles = new List<OHLC>();

        // Act & Assert - Should not throw
        _chartInteractions.BindCandles(emptyCandles);
        _chartInteractions.SnapEnabled = true;
        _chartInteractions.SnapMode = SnapMode.CandleOHLC;
    }

    private List<OHLC> GenerateSampleCandles(int count)
    {
        var candles = new List<OHLC>();
        var baseDate = new DateTime(2024, 1, 1);
        double basePrice = 100.0;

        for (int i = 0; i < count; i++)
        {
            var open = basePrice + (i * 0.5);
            var close = open + 1.0;
            var high = close + 0.5;
            var low = open - 0.5;

            candles.Add(new OHLC(
                open,
                high,
                low,
                close,
                baseDate.AddHours(i),
                TimeSpan.FromHours(1)
            ));
        }

        return candles;
    }
}
