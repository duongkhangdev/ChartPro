using ChartPro.Charting;
using ChartPro.Charting.Commands;
using ChartPro.Charting.Shapes;
using ScottPlot;

namespace ChartPro.Tests;

public class ShapeManagerTests
{
    [Fact]
    public void ShapeManager_InitialState_IsEmpty()
    {
        // Arrange & Act
        var manager = new ShapeManager();

        // Assert
        Assert.Empty(manager.Shapes);
        Assert.False(manager.CanUndo);
        Assert.False(manager.CanRedo);
    }

    [Fact]
    public void AddShape_AddsShapeToManager()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);

        // Act
        manager.AddShape(shape);

        // Assert
        Assert.Single(manager.Shapes);
        Assert.Equal(shape, manager.Shapes[0]);
    }

    [Fact]
    public void RemoveShape_RemovesShapeFromManager()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);
        manager.AddShape(shape);

        // Act
        manager.RemoveShape(shape);

        // Assert
        Assert.Empty(manager.Shapes);
    }

    [Fact]
    public void GetShapeById_ReturnsCorrectShape()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);
        manager.AddShape(shape);

        // Act
        var found = manager.GetShapeById(shape.Id);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(shape, found);
    }

    [Fact]
    public void GetShapeById_ReturnsNullForInvalidId()
    {
        // Arrange
        var manager = new ShapeManager();

        // Act
        var found = manager.GetShapeById(Guid.NewGuid());

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public void Clear_RemovesAllShapesAndHistory()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);
        var command = new AddShapeCommand(manager, shape, plot);
        manager.ExecuteCommand(command);

        // Act
        manager.Clear();

        // Assert
        Assert.Empty(manager.Shapes);
        Assert.False(manager.CanUndo);
        Assert.False(manager.CanRedo);
    }
}
