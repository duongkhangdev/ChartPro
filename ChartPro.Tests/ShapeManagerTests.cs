using ChartPro.Charting.ShapeManagement;
using ChartPro.Charting.Commands;
using ScottPlot;
using ScottPlot.WinForms;

namespace ChartPro.Tests;

public class ShapeManagerTests : IDisposable
{
    private readonly FormsPlot _formsPlot;
    private readonly ShapeManager _shapeManager;

    public ShapeManagerTests()
    {
        _formsPlot = new FormsPlot();
        _shapeManager = new ShapeManager();
        _shapeManager.Attach(_formsPlot);
    }

    public void Dispose()
    {
        _shapeManager?.Dispose();
        _formsPlot?.Dispose();
    }

    [Fact]
    public void ShapeManager_CanAttach()
    {
        // Arrange
        var shapeManager = new ShapeManager();
        var formsPlot = new FormsPlot();

        // Act
        shapeManager.Attach(formsPlot);

        // Assert
        Assert.True(shapeManager.IsAttached);

        // Cleanup
        shapeManager.Dispose();
        formsPlot.Dispose();
    }

    [Fact]
    public void ShapeManager_ThrowsExceptionWhenAttachedTwice()
    {
        // Arrange
        var shapeManager = new ShapeManager();
        var formsPlot = new FormsPlot();
        shapeManager.Attach(formsPlot);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shapeManager.Attach(formsPlot));

        // Cleanup
        shapeManager.Dispose();
        formsPlot.Dispose();
    }

    [Fact]
    public void AddShape_AddsShapeToChart()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);

        // Act
        _shapeManager.AddShape(line);

        // Assert
        Assert.Single(_shapeManager.Shapes);
        Assert.Contains(line, _shapeManager.Shapes);
    }

    [Fact]
    public void AddShape_EnablesUndo()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);

        // Act
        _shapeManager.AddShape(line);

        // Assert
        Assert.True(_shapeManager.CanUndo);
        Assert.False(_shapeManager.CanRedo);
    }

    [Fact]
    public void Undo_RemovesLastShape()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        _shapeManager.AddShape(line);

        // Act
        var result = _shapeManager.Undo();

        // Assert
        Assert.True(result);
        Assert.Empty(_shapeManager.Shapes);
        Assert.False(_shapeManager.CanUndo);
        Assert.True(_shapeManager.CanRedo);
    }

    [Fact]
    public void Redo_RestoresUndoneShape()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        _shapeManager.AddShape(line);
        _shapeManager.Undo();

        // Act
        var result = _shapeManager.Redo();

        // Assert
        Assert.True(result);
        Assert.Single(_shapeManager.Shapes);
        Assert.Contains(line, _shapeManager.Shapes);
        Assert.True(_shapeManager.CanUndo);
        Assert.False(_shapeManager.CanRedo);
    }

    [Fact]
    public void Undo_ReturnsFalseWhenNothingToUndo()
    {
        // Act
        var result = _shapeManager.Undo();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Redo_ReturnsFalseWhenNothingToRedo()
    {
        // Act
        var result = _shapeManager.Redo();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AddShape_ClearsRedoStack()
    {
        // Arrange
        var line1 = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var line2 = _formsPlot.Plot.Add.Line(5, 5, 15, 15);
        _shapeManager.AddShape(line1);
        _shapeManager.Undo();

        // Act
        _shapeManager.AddShape(line2);

        // Assert
        Assert.False(_shapeManager.CanRedo);
        Assert.Single(_shapeManager.Shapes);
    }

    [Fact]
    public void MultipleShapes_UndoRedoSequence()
    {
        // Arrange
        var line1 = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var line2 = _formsPlot.Plot.Add.Line(5, 5, 15, 15);
        var line3 = _formsPlot.Plot.Add.Line(10, 10, 20, 20);

        // Act
        _shapeManager.AddShape(line1);
        _shapeManager.AddShape(line2);
        _shapeManager.AddShape(line3);

        // Assert - all shapes added
        Assert.Equal(3, _shapeManager.Shapes.Count);

        // Act - undo twice
        _shapeManager.Undo();
        _shapeManager.Undo();

        // Assert - only first shape remains
        Assert.Single(_shapeManager.Shapes);
        Assert.Contains(line1, _shapeManager.Shapes);

        // Act - redo once
        _shapeManager.Redo();

        // Assert - two shapes now
        Assert.Equal(2, _shapeManager.Shapes.Count);
        Assert.Contains(line1, _shapeManager.Shapes);
        Assert.Contains(line2, _shapeManager.Shapes);
    }

    [Fact]
    public void DeleteShape_RemovesShapeFromChart()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        _shapeManager.AddShape(line);

        // Act
        _shapeManager.DeleteShape(line);

        // Assert
        Assert.Empty(_shapeManager.Shapes);
        Assert.True(_shapeManager.CanUndo);
    }

    [Fact]
    public void DeleteShape_UndoRestoresShape()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        _shapeManager.AddShape(line);
        _shapeManager.DeleteShape(line);

        // Act
        _shapeManager.Undo();

        // Assert
        Assert.Single(_shapeManager.Shapes);
        Assert.Contains(line, _shapeManager.Shapes);
    }

    [Fact]
    public void DeleteShape_ThrowsExceptionForUnmanagedShape()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _shapeManager.DeleteShape(line));
    }
}
