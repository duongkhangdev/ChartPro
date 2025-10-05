using ChartPro.Charting;
using ChartPro.Charting.Shapes;
using ScottPlot;

namespace ChartPro.Tests;

public class DrawnShapeTests
{
    [Fact]
    public void DrawnShape_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var drawMode = ChartDrawMode.TrendLine;

        // Act
        var shape = new DrawnShape(line, drawMode);

        // Assert
        Assert.NotEqual(Guid.Empty, shape.Id);
        Assert.Equal(line, shape.Plottable);
        Assert.Equal(drawMode, shape.DrawMode);
        Assert.True(shape.IsVisible);
        Assert.False(shape.IsSelected);
        Assert.True(shape.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void DrawnShape_IsSelected_CanBeModified()
    {
        // Arrange
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);

        // Act
        shape.IsSelected = true;

        // Assert
        Assert.True(shape.IsSelected);
    }

    [Fact]
    public void DrawnShape_IsVisible_CanBeModified()
    {
        // Arrange
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);

        // Act
        shape.IsVisible = false;

        // Assert
        Assert.False(shape.IsVisible);
    }

    [Fact]
    public void DrawnShape_Constructor_ThrowsOnNullPlottable()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DrawnShape(null!, ChartDrawMode.TrendLine));
    }

    [Fact]
    public void DrawnShape_EachInstanceHasUniqueId()
    {
        // Arrange
        var plot = new Plot();
        var line1 = plot.Add.Line(0, 0, 10, 10);
        var line2 = plot.Add.Line(5, 5, 15, 15);

        // Act
        var shape1 = new DrawnShape(line1, ChartDrawMode.TrendLine);
        var shape2 = new DrawnShape(line2, ChartDrawMode.TrendLine);

        // Assert
        Assert.NotEqual(shape1.Id, shape2.Id);
    }
}
