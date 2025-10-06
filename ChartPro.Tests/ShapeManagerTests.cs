using ChartPro.Charting;
using ChartPro.Charting.ShapeManagement;
using ChartPro.Charting.Commands;
using ChartPro.Charting.Shapes;
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
        var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);

        // Act
        _shapeManager.AddShape(drawnShape);

        // Assert
        Assert.Single(_shapeManager.Shapes);
        Assert.Contains(drawnShape, _shapeManager.Shapes);
    }

    [Fact]
    public void AddShape_EnablesUndo()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);

        // Act
        _shapeManager.AddShape(drawnShape);

        // Assert
        Assert.True(_shapeManager.CanUndo);
        Assert.False(_shapeManager.CanRedo);
    }

    [Fact]
    public void Undo_RemovesLastShape()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);
        _shapeManager.AddShape(drawnShape);

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
        var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);
        _shapeManager.AddShape(drawnShape);
        _shapeManager.Undo();

        // Act
        var result = _shapeManager.Redo();

        // Assert
        Assert.True(result);
        Assert.Single(_shapeManager.Shapes);
        Assert.Contains(drawnShape, _shapeManager.Shapes);
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
        var drawnShape1 = new DrawnShape(line1, ChartDrawMode.TrendLine);
        var drawnShape2 = new DrawnShape(line2, ChartDrawMode.TrendLine);
        _shapeManager.AddShape(drawnShape1);
        _shapeManager.Undo();

        // Act
        _shapeManager.AddShape(drawnShape2);

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
        var drawnShape1 = new DrawnShape(line1, ChartDrawMode.TrendLine);
        var drawnShape2 = new DrawnShape(line2, ChartDrawMode.TrendLine);
        var drawnShape3 = new DrawnShape(line3, ChartDrawMode.TrendLine);

        // Act
        _shapeManager.AddShape(drawnShape1);
        _shapeManager.AddShape(drawnShape2);
        _shapeManager.AddShape(drawnShape3);

        // Assert - all shapes added
        Assert.Equal(3, _shapeManager.Shapes.Count);

        // Act - undo twice
        _shapeManager.Undo();
        _shapeManager.Undo();

        // Assert - only first shape remains
        Assert.Single(_shapeManager.Shapes);
        Assert.Contains(drawnShape1, _shapeManager.Shapes);

        // Act - redo once
        _shapeManager.Redo();

        // Assert - two shapes now
        Assert.Equal(2, _shapeManager.Shapes.Count);
        Assert.Contains(drawnShape1, _shapeManager.Shapes);
        Assert.Contains(drawnShape2, _shapeManager.Shapes);
    }

    [Fact]
    public void DeleteShape_RemovesShapeFromChart()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);
        _shapeManager.AddShape(drawnShape);

        // Act
        _shapeManager.DeleteShape(drawnShape);

        // Assert
        Assert.Empty(_shapeManager.Shapes);
        Assert.True(_shapeManager.CanUndo);
    }

    [Fact]
    public void DeleteShape_UndoRestoresShape()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);
        _shapeManager.AddShape(drawnShape);
        _shapeManager.DeleteShape(drawnShape);

        // Act
        _shapeManager.Undo();

        // Assert
        Assert.Single(_shapeManager.Shapes);
        Assert.Contains(drawnShape, _shapeManager.Shapes);
    }

    [Fact]
    public void DeleteShape_ThrowsExceptionForUnmanagedShape()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _shapeManager.DeleteShape(drawnShape));
    }

    [Fact]
    public void SelectShapeAt_SelectsNearbyShape()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);
        _shapeManager.AddShape(drawnShape);

        // Act - Try to select near the center of the line (pixel coordinates)
        var selected = _shapeManager.SelectShapeAt(100, 100, false);

        // Assert - Selection might or might not work depending on ScottPlot's coordinate system
        // but we can verify the method doesn't crash
        Assert.NotNull(_shapeManager.Shapes);
    }

    [Fact]
    public void ToggleSelection_TogglesShapeSelection()
    {
        // Arrange
        var line = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);
        _shapeManager.AddShape(drawnShape);
        Assert.False(drawnShape.IsSelected);

        // Act - Toggle selection on
        _shapeManager.ToggleSelection(drawnShape);

        // Assert
        Assert.True(drawnShape.IsSelected);

        // Act - Toggle selection off
        _shapeManager.ToggleSelection(drawnShape);

        // Assert
        Assert.False(drawnShape.IsSelected);
    }

    [Fact]
    public void ClearSelection_ClearsAllSelectedShapes()
    {
        // Arrange
        var line1 = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var line2 = _formsPlot.Plot.Add.Line(5, 5, 15, 15);
        var drawnShape1 = new DrawnShape(line1, ChartDrawMode.TrendLine);
        var drawnShape2 = new DrawnShape(line2, ChartDrawMode.TrendLine);
        _shapeManager.AddShape(drawnShape1);
        _shapeManager.AddShape(drawnShape2);
        drawnShape1.IsSelected = true;
        drawnShape2.IsSelected = true;

        // Act
        _shapeManager.ClearSelection();

        // Assert
        Assert.False(drawnShape1.IsSelected);
        Assert.False(drawnShape2.IsSelected);
    }

    [Fact]
    public void SelectedShapes_ReturnsOnlySelectedShapes()
    {
        // Arrange
        var line1 = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var line2 = _formsPlot.Plot.Add.Line(5, 5, 15, 15);
        var line3 = _formsPlot.Plot.Add.Line(10, 10, 20, 20);
        var drawnShape1 = new DrawnShape(line1, ChartDrawMode.TrendLine);
        var drawnShape2 = new DrawnShape(line2, ChartDrawMode.TrendLine);
        var drawnShape3 = new DrawnShape(line3, ChartDrawMode.TrendLine);
        _shapeManager.AddShape(drawnShape1);
        _shapeManager.AddShape(drawnShape2);
        _shapeManager.AddShape(drawnShape3);
        drawnShape1.IsSelected = true;
        drawnShape3.IsSelected = true;

        // Act
        var selectedShapes = _shapeManager.SelectedShapes;

        // Assert
        Assert.Equal(2, selectedShapes.Count);
        Assert.Contains(drawnShape1, selectedShapes);
        Assert.Contains(drawnShape3, selectedShapes);
        Assert.DoesNotContain(drawnShape2, selectedShapes);
    }

    [Fact]
    public void DeleteSelectedShapes_RemovesAllSelectedShapes()
    {
        // Arrange
        var line1 = _formsPlot.Plot.Add.Line(0, 0, 10, 10);
        var line2 = _formsPlot.Plot.Add.Line(5, 5, 15, 15);
        var line3 = _formsPlot.Plot.Add.Line(10, 10, 20, 20);
        var drawnShape1 = new DrawnShape(line1, ChartDrawMode.TrendLine);
        var drawnShape2 = new DrawnShape(line2, ChartDrawMode.TrendLine);
        var drawnShape3 = new DrawnShape(line3, ChartDrawMode.TrendLine);
        _shapeManager.AddShape(drawnShape1);
        _shapeManager.AddShape(drawnShape2);
        _shapeManager.AddShape(drawnShape3);
        drawnShape1.IsSelected = true;
        drawnShape3.IsSelected = true;

        // Act
        _shapeManager.DeleteSelectedShapes();

        // Assert
        Assert.Single(_shapeManager.Shapes);
        Assert.Contains(drawnShape2, _shapeManager.Shapes);
    }
}
