using ChartPro.Charting.Commands;
using ScottPlot;
using ScottPlot.WinForms;

namespace ChartPro.Tests;

public class CommandTests : IDisposable
{
    private readonly FormsPlot _formsPlot;

    public CommandTests()
    {
        _formsPlot = new FormsPlot();
    }

    public void Dispose()
    {
        _formsPlot?.Dispose();
    }

    [Fact]
    public void AddShapeCommand_Execute_AddsShapeToPlot()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var command = new AddShapeCommand(_formsPlot, line);
        var initialCount = _formsPlot.Plot.GetPlottables().Count();

        // Act
        command.Execute();

        // Assert
        var plottables = _formsPlot.Plot.GetPlottables();
        Assert.Equal(initialCount + 1, plottables.Count());
        Assert.Contains(line, plottables);
    }

    [Fact]
    public void AddShapeCommand_Undo_RemovesShapeFromPlot()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var command = new AddShapeCommand(_formsPlot, line);
        command.Execute();
        var countAfterExecute = _formsPlot.Plot.GetPlottables().Count();

        // Act
        command.Undo();

        // Assert
        var plottables = _formsPlot.Plot.GetPlottables();
        Assert.Equal(countAfterExecute - 1, plottables.Count());
        Assert.DoesNotContain(line, plottables);
    }

    [Fact]
    public void DeleteShapeCommand_Execute_RemovesShapeFromPlot()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        _formsPlot.Plot.Add.Plottable(line);
        var command = new DeleteShapeCommand(_formsPlot, line);
        var initialCount = _formsPlot.Plot.GetPlottables().Count();

        // Act
        command.Execute();

        // Assert
        var plottables = _formsPlot.Plot.GetPlottables();
        Assert.Equal(initialCount - 1, plottables.Count());
        Assert.DoesNotContain(line, plottables);
    }

    [Fact]
    public void DeleteShapeCommand_Undo_RestoresShapeToPlot()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        _formsPlot.Plot.Add.Plottable(line);
        var command = new DeleteShapeCommand(_formsPlot, line);
        command.Execute();
        var countAfterExecute = _formsPlot.Plot.GetPlottables().Count();

        // Act
        command.Undo();

        // Assert
        var plottables = _formsPlot.Plot.GetPlottables();
        Assert.Equal(countAfterExecute + 1, plottables.Count());
        Assert.Contains(line, plottables);
    }

    [Fact]
    public void AddShapeCommand_ExecuteAndUndoMultipleTimes()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var command = new AddShapeCommand(_formsPlot, line);

        // Act & Assert - multiple execute/undo cycles
        for (int i = 0; i < 3; i++)
        {
            command.Execute();
            Assert.Contains(line, _formsPlot.Plot.GetPlottables());

            command.Undo();
            Assert.DoesNotContain(line, _formsPlot.Plot.GetPlottables());
        }
    }
}
