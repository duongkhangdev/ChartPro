using ChartPro.Charting;
using ChartPro.Charting.Commands;
using ChartPro.Charting.Shapes;
using ScottPlot;

namespace ChartPro.Tests;

public class CommandTests
{
    [Fact]
    public void AddShapeCommand_Execute_AddsShapeToManagerAndPlot()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);
        var command = new AddShapeCommand(manager, shape, plot);

        // Act
        command.Execute();

        // Assert
        Assert.Single(manager.Shapes);
        Assert.Contains(line, plot.PlottableList);
    }

    [Fact]
    public void AddShapeCommand_Undo_RemovesShapeFromManagerAndPlot()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);
        var command = new AddShapeCommand(manager, shape, plot);
        command.Execute();

        // Act
        command.Undo();

        // Assert
        Assert.Empty(manager.Shapes);
        Assert.DoesNotContain(line, plot.PlottableList);
    }

    [Fact]
    public void DeleteShapeCommand_Execute_RemovesShapeFromManagerAndPlot()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);
        manager.AddShape(shape);
        var command = new DeleteShapeCommand(manager, shape, plot);

        // Act
        command.Execute();

        // Assert
        Assert.Empty(manager.Shapes);
        Assert.DoesNotContain(line, plot.PlottableList);
    }

    [Fact]
    public void DeleteShapeCommand_Undo_AddsShapeBackToManagerAndPlot()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);
        manager.AddShape(shape);
        var command = new DeleteShapeCommand(manager, shape, plot);
        command.Execute();

        // Act
        command.Undo();

        // Assert
        Assert.Single(manager.Shapes);
        Assert.Contains(line, plot.PlottableList);
    }

    [Fact]
    public void ExecuteCommand_PushesToUndoStackAndClearsRedo()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);
        var command = new AddShapeCommand(manager, shape, plot);

        // Act
        manager.ExecuteCommand(command);

        // Assert
        Assert.True(manager.CanUndo);
        Assert.False(manager.CanRedo);
    }

    [Fact]
    public void Undo_MovesCommandToRedoStack()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);
        var command = new AddShapeCommand(manager, shape, plot);
        manager.ExecuteCommand(command);

        // Act
        var result = manager.Undo();

        // Assert
        Assert.True(result);
        Assert.False(manager.CanUndo);
        Assert.True(manager.CanRedo);
    }

    [Fact]
    public void Redo_MovesCommandBackToUndoStack()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        var line = plot.Add.Line(0, 0, 10, 10);
        var shape = new DrawnShape(line, ChartDrawMode.TrendLine);
        var command = new AddShapeCommand(manager, shape, plot);
        manager.ExecuteCommand(command);
        manager.Undo();

        // Act
        var result = manager.Redo();

        // Assert
        Assert.True(result);
        Assert.True(manager.CanUndo);
        Assert.False(manager.CanRedo);
    }

    [Fact]
    public void MultipleUndoRedo_WorksCorrectly()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        
        var line1 = plot.Add.Line(0, 0, 10, 10);
        var shape1 = new DrawnShape(line1, ChartDrawMode.TrendLine);
        var command1 = new AddShapeCommand(manager, shape1, plot);
        
        var line2 = plot.Add.Line(5, 5, 15, 15);
        var shape2 = new DrawnShape(line2, ChartDrawMode.TrendLine);
        var command2 = new AddShapeCommand(manager, shape2, plot);

        // Act & Assert
        manager.ExecuteCommand(command1);
        Assert.Single(manager.Shapes);
        
        manager.ExecuteCommand(command2);
        Assert.Equal(2, manager.Shapes.Count);
        
        manager.Undo();
        Assert.Single(manager.Shapes);
        
        manager.Undo();
        Assert.Empty(manager.Shapes);
        
        manager.Redo();
        Assert.Single(manager.Shapes);
        
        manager.Redo();
        Assert.Equal(2, manager.Shapes.Count);
    }

    [Fact]
    public void NewCommand_ClearsRedoStack()
    {
        // Arrange
        var manager = new ShapeManager();
        var plot = new Plot();
        
        var line1 = plot.Add.Line(0, 0, 10, 10);
        var shape1 = new DrawnShape(line1, ChartDrawMode.TrendLine);
        var command1 = new AddShapeCommand(manager, shape1, plot);
        
        var line2 = plot.Add.Line(5, 5, 15, 15);
        var shape2 = new DrawnShape(line2, ChartDrawMode.TrendLine);
        var command2 = new AddShapeCommand(manager, shape2, plot);

        // Act
        manager.ExecuteCommand(command1);
        manager.Undo();
        Assert.True(manager.CanRedo);
        
        manager.ExecuteCommand(command2);

        // Assert
        Assert.False(manager.CanRedo);
        Assert.True(manager.CanUndo);
    }
}
