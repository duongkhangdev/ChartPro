using ChartPro.Charting;
using ChartPro.Charting.Interactions;
using ChartPro.Charting.ShapeManagement;
using ChartPro.Charting.Models;
using ScottPlot;
using ScottPlot.WinForms;
using System.Text.Json;

namespace ChartPro.Tests;

/// <summary>
/// Tests for annotation persistence (save/load) functionality.
/// </summary>
public class AnnotationPersistenceTests : IDisposable
{
    private readonly FormsPlot _formsPlot;
    private readonly ChartInteractions _chartInteractions;
    private readonly string _testFilePath;

    public AnnotationPersistenceTests()
    {
        _formsPlot = new FormsPlot();
        var shapeManager = new ShapeManager();
        _chartInteractions = new ChartInteractions(shapeManager);
        _chartInteractions.Attach(_formsPlot);
        
        // Create a temporary file path for testing
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test_annotations_{Guid.NewGuid()}.json");
    }

    public void Dispose()
    {
        // Clean up test file if it exists
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
        
        _chartInteractions?.Dispose();
        _formsPlot?.Dispose();
    }

    [Fact]
    public void SaveShapesToFile_CreatesValidJsonFile()
    {
        // Arrange
        _chartInteractions.SetDrawMode(ChartDrawMode.TrendLine);
        
        // Simulate drawing a trend line
        var mouseDown = typeof(ChartInteractions).GetMethod("OnMouseDown", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var mouseUp = typeof(ChartInteractions).GetMethod("OnMouseUp", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        mouseDown?.Invoke(_chartInteractions, new object[] { null!, new MouseEventArgs(MouseButtons.Left, 1, 100, 100, 0) });
        mouseUp?.Invoke(_chartInteractions, new object[] { null!, new MouseEventArgs(MouseButtons.Left, 1, 200, 150, 0) });

        // Act
        _chartInteractions.SaveShapesToFile(_testFilePath);

        // Assert
        Assert.True(File.Exists(_testFilePath));
        
        var json = File.ReadAllText(_testFilePath);
        var annotations = JsonSerializer.Deserialize<ChartAnnotations>(json);
        
        Assert.NotNull(annotations);
        Assert.Equal(1, annotations.Version);
        Assert.NotEmpty(annotations.Shapes);
    }

    [Fact]
    public void SaveShapesToFile_SavesEmptyShapesList()
    {
        // Act - Save with no shapes drawn
        _chartInteractions.SaveShapesToFile(_testFilePath);

        // Assert
        Assert.True(File.Exists(_testFilePath));
        
        var json = File.ReadAllText(_testFilePath);
        var annotations = JsonSerializer.Deserialize<ChartAnnotations>(json);
        
        Assert.NotNull(annotations);
        Assert.Equal(1, annotations.Version);
        Assert.Empty(annotations.Shapes);
    }

    [Fact]
    public void LoadShapesFromFile_ThrowsExceptionWhenFileNotFound()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.json");

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => 
            _chartInteractions.LoadShapesFromFile(nonExistentFile));
        
        Assert.Contains("Annotations file not found", exception.Message);
    }

    [Fact]
    public void LoadShapesFromFile_ThrowsExceptionWhenNotAttached()
    {
        // Arrange
        var shapeManager = new ShapeManager();
        var unattachedInteractions = new ChartInteractions(shapeManager);
        
        // Create a valid test file
        File.WriteAllText(_testFilePath, "{\"Version\":1,\"Shapes\":[]}");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            unattachedInteractions.LoadShapesFromFile(_testFilePath));
        
        Assert.Contains("Chart is not attached", exception.Message);
        
        unattachedInteractions.Dispose();
    }

    [Fact]
    public void LoadShapesFromFile_HandlesInvalidJson()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "{ invalid json content }");

        // Act & Assert
        Assert.Throws<JsonException>(() => 
            _chartInteractions.LoadShapesFromFile(_testFilePath));
    }

    [Fact]
    public void LoadShapesFromFile_HandlesNullAnnotations()
    {
        // Arrange - Write null JSON
        File.WriteAllText(_testFilePath, "null");

        // Act - Should not throw, just return early
        _chartInteractions.LoadShapesFromFile(_testFilePath);

        // Assert - No shapes should be added
        Assert.Empty(_chartInteractions.ShapeManager.Shapes);
    }

    [Fact]
    public void LoadShapesFromFile_HandlesEmptyShapesList()
    {
        // Arrange
        var annotations = new ChartAnnotations { Version = 1, Shapes = new List<ShapeAnnotation>() };
        var json = JsonSerializer.Serialize(annotations);
        File.WriteAllText(_testFilePath, json);

        // Act
        _chartInteractions.LoadShapesFromFile(_testFilePath);

        // Assert - No shapes should be added
        Assert.Empty(_chartInteractions.ShapeManager.Shapes);
    }

    [Fact]
    public void SaveAndLoad_PreservesShapeData()
    {
        // Arrange - Create annotations with different shape types
        var annotations = new ChartAnnotations
        {
            Version = 1,
            Shapes = new List<ShapeAnnotation>
            {
                new ShapeAnnotation
                {
                    ShapeType = "TrendLine",
                    X1 = 10.0, Y1 = 100.0,
                    X2 = 50.0, Y2 = 110.0,
                    LineColor = "#0000FF",
                    LineWidth = 2
                },
                new ShapeAnnotation
                {
                    ShapeType = "Rectangle",
                    X1 = 20.0, Y1 = 95.0,
                    X2 = 40.0, Y2 = 105.0,
                    LineColor = "#800080",
                    LineWidth = 2,
                    FillColor = "#800080",
                    FillAlpha = 25
                }
            }
        };
        
        var json = JsonSerializer.Serialize(annotations, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_testFilePath, json);

        // Act
        _chartInteractions.LoadShapesFromFile(_testFilePath);

        // Assert - Shapes are loaded (check via ShapeManager since _drawnShapes is private)
        // We can't directly verify _drawnShapes but we can save and compare
        var saveFilePath = Path.Combine(Path.GetTempPath(), $"test_save_{Guid.NewGuid()}.json");
        try
        {
            _chartInteractions.SaveShapesToFile(saveFilePath);
            var savedJson = File.ReadAllText(saveFilePath);
            var savedAnnotations = JsonSerializer.Deserialize<ChartAnnotations>(savedJson);
            
            Assert.NotNull(savedAnnotations);
            Assert.Equal(2, savedAnnotations.Shapes.Count);
            
            // Verify first shape (TrendLine)
            var trendLine = savedAnnotations.Shapes[0];
            Assert.Equal("TrendLine", trendLine.ShapeType);
            Assert.Equal(10.0, trendLine.X1);
            Assert.Equal(100.0, trendLine.Y1);
            Assert.Equal(50.0, trendLine.X2);
            Assert.Equal(110.0, trendLine.Y2);
            
            // Verify second shape (Rectangle)
            var rectangle = savedAnnotations.Shapes[1];
            Assert.Equal("Rectangle", rectangle.ShapeType);
            Assert.Equal(20.0, rectangle.X1);
            Assert.Equal(95.0, rectangle.Y1);
            Assert.Equal(40.0, rectangle.X2);
            Assert.Equal(105.0, rectangle.Y2);
        }
        finally
        {
            if (File.Exists(saveFilePath))
                File.Delete(saveFilePath);
        }
    }

    [Fact]
    public void LoadShapesFromFile_ClearsExistingShapes()
    {
        // Arrange - Draw a shape first
        _chartInteractions.SetDrawMode(ChartDrawMode.TrendLine);
        var mouseDown = typeof(ChartInteractions).GetMethod("OnMouseDown", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var mouseUp = typeof(ChartInteractions).GetMethod("OnMouseUp", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        mouseDown?.Invoke(_chartInteractions, new object[] { null!, new MouseEventArgs(MouseButtons.Left, 1, 100, 100, 0) });
        mouseUp?.Invoke(_chartInteractions, new object[] { null!, new MouseEventArgs(MouseButtons.Left, 1, 200, 150, 0) });
        
        // Create a file with different shapes
        var annotations = new ChartAnnotations
        {
            Version = 1,
            Shapes = new List<ShapeAnnotation>
            {
                new ShapeAnnotation
                {
                    ShapeType = "Circle",
                    X1 = 35.0, Y1 = 98.0,
                    X2 = 45.0, Y2 = 108.0,
                    LineColor = "#00FFFF",
                    LineWidth = 2
                }
            }
        };
        var json = JsonSerializer.Serialize(annotations);
        File.WriteAllText(_testFilePath, json);

        // Act - Load should clear existing and load new
        _chartInteractions.LoadShapesFromFile(_testFilePath);
        
        // Save and verify only the loaded shape exists
        var saveFilePath = Path.Combine(Path.GetTempPath(), $"test_clear_{Guid.NewGuid()}.json");
        try
        {
            _chartInteractions.SaveShapesToFile(saveFilePath);
            var savedJson = File.ReadAllText(saveFilePath);
            var savedAnnotations = JsonSerializer.Deserialize<ChartAnnotations>(savedJson);
            
            Assert.NotNull(savedAnnotations);
            Assert.Single(savedAnnotations.Shapes);
            Assert.Equal("Circle", savedAnnotations.Shapes[0].ShapeType);
        }
        finally
        {
            if (File.Exists(saveFilePath))
                File.Delete(saveFilePath);
        }
    }

    [Fact]
    public void LoadShapesFromFile_SupportsAllShapeTypes()
    {
        // Arrange - Create file with all supported shape types
        var annotations = new ChartAnnotations
        {
            Version = 1,
            Shapes = new List<ShapeAnnotation>
            {
                new ShapeAnnotation { ShapeType = "TrendLine", X1 = 10, Y1 = 100, X2 = 50, Y2 = 110 },
                new ShapeAnnotation { ShapeType = "HorizontalLine", X1 = 0, Y1 = 102.5, X2 = 100, Y2 = 102.5 },
                new ShapeAnnotation { ShapeType = "VerticalLine", X1 = 30, Y1 = 90, X2 = 30, Y2 = 115 },
                new ShapeAnnotation { ShapeType = "Rectangle", X1 = 20, Y1 = 95, X2 = 40, Y2 = 105 },
                new ShapeAnnotation { ShapeType = "Circle", X1 = 35, Y1 = 98, X2 = 45, Y2 = 108 },
                new ShapeAnnotation { ShapeType = "FibonacciRetracement", X1 = 15, Y1 = 95, X2 = 55, Y2 = 112 }
            }
        };
        var json = JsonSerializer.Serialize(annotations);
        File.WriteAllText(_testFilePath, json);

        // Act
        _chartInteractions.LoadShapesFromFile(_testFilePath);

        // Assert
        var saveFilePath = Path.Combine(Path.GetTempPath(), $"test_all_shapes_{Guid.NewGuid()}.json");
        try
        {
            _chartInteractions.SaveShapesToFile(saveFilePath);
            var savedJson = File.ReadAllText(saveFilePath);
            var savedAnnotations = JsonSerializer.Deserialize<ChartAnnotations>(savedJson);
            
            Assert.NotNull(savedAnnotations);
            Assert.Equal(6, savedAnnotations.Shapes.Count);
            
            // Verify all shape types are present
            var shapeTypes = savedAnnotations.Shapes.Select(s => s.ShapeType).ToList();
            Assert.Contains("TrendLine", shapeTypes);
            Assert.Contains("HorizontalLine", shapeTypes);
            Assert.Contains("VerticalLine", shapeTypes);
            Assert.Contains("Rectangle", shapeTypes);
            Assert.Contains("Circle", shapeTypes);
            Assert.Contains("FibonacciRetracement", shapeTypes);
        }
        finally
        {
            if (File.Exists(saveFilePath))
                File.Delete(saveFilePath);
        }
    }

    [Fact]
    public void LoadShapesFromFile_IgnoresInvalidShapeTypes()
    {
        // Arrange - Create file with invalid shape type
        var annotations = new ChartAnnotations
        {
            Version = 1,
            Shapes = new List<ShapeAnnotation>
            {
                new ShapeAnnotation { ShapeType = "InvalidShapeType", X1 = 10, Y1 = 100, X2 = 50, Y2 = 110 },
                new ShapeAnnotation { ShapeType = "TrendLine", X1 = 20, Y1 = 100, X2 = 60, Y2 = 110 }
            }
        };
        var json = JsonSerializer.Serialize(annotations);
        File.WriteAllText(_testFilePath, json);

        // Act - Should not throw, just skip invalid shapes
        _chartInteractions.LoadShapesFromFile(_testFilePath);

        // Assert - Only valid shape should be loaded
        var saveFilePath = Path.Combine(Path.GetTempPath(), $"test_invalid_{Guid.NewGuid()}.json");
        try
        {
            _chartInteractions.SaveShapesToFile(saveFilePath);
            var savedJson = File.ReadAllText(saveFilePath);
            var savedAnnotations = JsonSerializer.Deserialize<ChartAnnotations>(savedJson);
            
            Assert.NotNull(savedAnnotations);
            Assert.Single(savedAnnotations.Shapes);
            Assert.Equal("TrendLine", savedAnnotations.Shapes[0].ShapeType);
        }
        finally
        {
            if (File.Exists(saveFilePath))
                File.Delete(saveFilePath);
        }
    }
}
