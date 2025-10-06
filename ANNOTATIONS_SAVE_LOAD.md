# Save/Load Annotations Feature

This document describes the save and load annotations functionality for chart shapes in ChartPro.

## Overview

The save/load annotations feature allows users to persist all drawn shapes (trend lines, rectangles, circles, Fibonacci retracements, etc.) to a JSON file and reload them later. This is useful for:
- Saving technical analysis work
- Sharing chart setups with others
- Creating templates for common trading scenarios
- Backing up your annotations

## User Interface

### Save Annotations
1. Click the **"Save Annotations"** button in the right toolbar
2. A file dialog will appear prompting for a save location
3. Choose a filename (default: `annotations.json`)
4. Click Save
5. A success message will confirm the save operation

### Load Annotations
1. Click the **"Load Annotations"** button in the right toolbar
2. A file dialog will appear prompting for a file to open
3. Select a previously saved `.json` file
4. Click Open
5. All existing shapes will be cleared and replaced with the loaded shapes
6. A success message will confirm the load operation

## File Format

Annotations are saved in JSON format with the following structure:

```json
{
  "Version": 1,
  "Shapes": [
    {
      "ShapeType": "TrendLine",
      "X1": 10.0,
      "Y1": 100.0,
      "X2": 50.0,
      "Y2": 110.0,
      "LineColor": "#0000FF",
      "LineWidth": 2,
      "FillColor": null,
      "FillAlpha": 25
    },
    {
      "ShapeType": "Rectangle",
      "X1": 20.0,
      "Y1": 95.0,
      "X2": 40.0,
      "Y2": 105.0,
      "LineColor": "#800080",
      "LineWidth": 2,
      "FillColor": "#800080",
      "FillAlpha": 25
    }
  ]
}
```

### Field Descriptions

- **Version**: Format version for future compatibility (currently 1)
- **Shapes**: Array of shape objects

Each shape has the following properties:
- **ShapeType**: Type of shape (TrendLine, HorizontalLine, VerticalLine, Rectangle, Circle, FibonacciRetracement)
- **X1, Y1**: Starting coordinates
- **X2, Y2**: Ending coordinates
- **LineColor**: Line color in hex format (e.g., "#0000FF" for blue)
- **LineWidth**: Line width in pixels
- **FillColor**: Fill color in hex format (optional, null for shapes without fill)
- **FillAlpha**: Alpha transparency for fill (0-255, default: 25)

## Supported Shape Types

The following shape types are currently supported:
- **TrendLine**: Diagonal lines for trend analysis
- **HorizontalLine**: Horizontal support/resistance lines
- **VerticalLine**: Vertical time-based lines
- **Rectangle**: Rectangular areas
- **Circle**: Circular/elliptical areas
- **FibonacciRetracement**: Fibonacci retracement levels

## Error Handling

The feature includes comprehensive error handling for common scenarios:

### Save Errors
- **General save failure**: If the file cannot be written (e.g., permission denied, disk full), an error message is displayed

### Load Errors
- **File not found**: If the selected file doesn't exist
- **Invalid JSON format**: If the file contains malformed JSON
- **Chart not attached**: If the chart control hasn't been initialized (internal error)
- **General load failure**: For other unexpected errors

All error messages are user-friendly and displayed in dialog boxes.

## Implementation Details

### Backend (ChartInteractions.cs)
The core save/load logic is implemented in the `ChartInteractions` class:

```csharp
// Save shapes to file
public void SaveShapesToFile(string filePath)
{
    var annotations = new ChartAnnotations
    {
        Version = 1,
        Shapes = _drawnShapes.Select(s => s.metadata).ToList()
    };
    
    var options = new JsonSerializerOptions { WriteIndented = true };
    var json = JsonSerializer.Serialize(annotations, options);
    File.WriteAllText(filePath, json);
}

// Load shapes from file
public void LoadShapesFromFile(string filePath)
{
    // Validation
    if (!File.Exists(filePath))
        throw new FileNotFoundException("Annotations file not found.", filePath);
    
    // Deserialize
    var json = File.ReadAllText(filePath);
    var annotations = JsonSerializer.Deserialize<ChartAnnotations>(json);
    
    // Clear existing shapes
    foreach (var (plottable, _) in _drawnShapes)
        _formsPlot.Plot.Remove(plottable);
    _drawnShapes.Clear();
    
    // Load new shapes
    foreach (var shape in annotations.Shapes)
    {
        // Create plottable from shape data
        // Add to chart and tracking list
    }
    
    _formsPlot.Refresh();
}
```

### Frontend (MainForm.cs)
The UI integration uses standard Windows file dialogs:

```csharp
private void SaveAnnotations()
{
    using var saveFileDialog = new SaveFileDialog
    {
        Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
        Title = "Save Annotations",
        DefaultExt = "json",
        FileName = "annotations.json"
    };
    
    if (saveFileDialog.ShowDialog() == DialogResult.OK)
    {
        try
        {
            _chartInteractions.SaveShapesToFile(saveFileDialog.FileName);
            MessageBox.Show("Annotations saved successfully", "Success");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save: {ex.Message}", "Error");
        }
    }
}
```

## Testing

Comprehensive unit tests are provided in `ChartPro.Tests/AnnotationPersistenceTests.cs`:

- ✅ Save creates valid JSON file
- ✅ Save handles empty shapes list
- ✅ Load throws exception for missing file
- ✅ Load throws exception when not attached
- ✅ Load handles invalid JSON
- ✅ Load handles null annotations
- ✅ Load handles empty shapes list
- ✅ Save/load preserves shape data
- ✅ Load clears existing shapes
- ✅ Load supports all shape types
- ✅ Load ignores invalid shape types

## Usage Examples

### Example 1: Save Current Work
1. Draw several shapes on the chart (trend lines, Fibonacci levels, etc.)
2. Click "Save Annotations"
3. Save to `my-analysis.json`
4. Close the application

### Example 2: Load Previous Work
1. Open ChartPro
2. Click "Load Annotations"
3. Select `my-analysis.json`
4. All your previous shapes are restored

### Example 3: Share Analysis
1. Complete your technical analysis with various shapes
2. Save to `eurusd-analysis.json`
3. Send the file to a colleague
4. They can load it in their ChartPro to see your analysis

## Limitations

- Loading annotations will **clear all existing shapes** on the chart
- Coordinates are absolute and may not align perfectly if loaded on different chart data
- The feature doesn't save chart data (OHLC candles), only the drawn shapes
- Undo/redo history is cleared when loading annotations

## Future Enhancements

Potential improvements for future versions:
- **Append mode**: Load annotations without clearing existing shapes
- **Selective loading**: Choose which shapes to load
- **Metadata**: Add timestamps, author, description to annotation files
- **Import/export formats**: Support for other formats (CSV, XML)
- **Templates**: Pre-defined annotation templates for common patterns
- **Auto-save**: Automatic periodic saving of annotations

## Troubleshooting

**Q: My shapes disappeared after loading annotations**  
A: This is expected behavior. Loading clears all existing shapes. Save before loading if you want to preserve current work.

**Q: The loaded shapes are in the wrong position**  
A: Annotation coordinates are absolute. Ensure you're loading annotations that were created on the same chart data or similar data range.

**Q: I get an "Invalid JSON format" error**  
A: The file may be corrupted or edited incorrectly. Try opening it in a text editor to verify the JSON structure matches the format described above.

**Q: Can I edit the JSON file manually?**  
A: Yes, but be careful to maintain valid JSON syntax and use the correct structure. Any errors will cause loading to fail.
