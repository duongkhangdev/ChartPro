# Shape Selection API Documentation

## Overview

The shape selection feature enables users to interact with drawn shapes on the chart by clicking to select them, toggling selection states, and performing operations on selected shapes. This is implemented through the `IShapeManager` interface and the `DrawnShape` metadata wrapper class.

## Architecture

### Key Components

1. **DrawnShape**: Wraps `IPlottable` with metadata including:
   - `Id`: Unique identifier (Guid)
   - `Plottable`: The underlying ScottPlot IPlottable object
   - `DrawMode`: The drawing mode used to create the shape
   - `IsSelected`: Selection state (boolean)
   - `IsVisible`: Visibility state (boolean)
   - `CreatedAt`: Creation timestamp

2. **IShapeManager**: Extended interface with selection methods:
   - `SelectShapeAt()`: Select a shape at pixel coordinates
   - `ToggleSelection()`: Toggle selection state of a specific shape
   - `ClearSelection()`: Clear all selections
   - `SelectedShapes`: Get all currently selected shapes
   - `DeleteSelectedShapes()`: Delete all selected shapes

3. **ChartInteractions**: Updated to handle shape selection via mouse clicks

## API Reference

### IShapeManager Methods

#### SelectShapeAt
```csharp
DrawnShape? SelectShapeAt(int pixelX, int pixelY, bool addToSelection = false)
```

Selects a shape at the given pixel coordinates.

**Parameters:**
- `pixelX`: The X pixel coordinate
- `pixelY`: The Y pixel coordinate  
- `addToSelection`: If true, adds to existing selection; if false, replaces selection

**Returns:** The selected shape, or null if no shape was found

**Example:**
```csharp
// Select a shape at mouse click position
var selected = shapeManager.SelectShapeAt(e.X, e.Y, false);
if (selected != null)
{
    Console.WriteLine($"Selected shape: {selected.Id}");
}
```

#### ToggleSelection
```csharp
void ToggleSelection(DrawnShape shape)
```

Toggles the selection state of a specific shape.

**Parameters:**
- `shape`: The shape to toggle

**Throws:** `InvalidOperationException` if the shape is not managed

**Example:**
```csharp
var shape = shapeManager.Shapes.First();
shapeManager.ToggleSelection(shape); // Select
shapeManager.ToggleSelection(shape); // Deselect
```

#### ClearSelection
```csharp
void ClearSelection()
```

Clears all selections.

**Example:**
```csharp
shapeManager.ClearSelection();
```

#### DeleteSelectedShapes
```csharp
void DeleteSelectedShapes()
```

Deletes all currently selected shapes using the command pattern (supports undo/redo).

**Example:**
```csharp
shapeManager.DeleteSelectedShapes();
formsPlot.Refresh();
```

### Properties

#### SelectedShapes
```csharp
IReadOnlyList<DrawnShape> SelectedShapes { get; }
```

Gets all currently selected shapes.

**Example:**
```csharp
var selectedCount = shapeManager.SelectedShapes.Count;
foreach (var shape in shapeManager.SelectedShapes)
{
    Console.WriteLine($"Selected: {shape.DrawMode} at {shape.CreatedAt}");
}
```

## User Interaction

### Mouse Selection

Users can select shapes by clicking on them when no drawing mode is active:

1. **Single Selection**: Click on a shape to select it (deselects all others)
2. **Multi-Selection**: Hold `Ctrl` and click to add shapes to selection
3. **Deselect All**: Click on empty space without `Ctrl` to clear selection

### Visual Feedback

Selected shapes are visually distinguished by:
- Increased line width (1.5x the original width)
- Highlighted color (yellow) for better visibility

### Keyboard Shortcuts

- `Delete`: Delete selected shapes (when implemented in UI)
- `Ctrl+Z`: Undo last operation (including deletions)
- `Ctrl+Y`: Redo last undone operation

## Usage Examples

### Basic Selection Flow

```csharp
// Initialize
var shapeManager = new ShapeManager();
shapeManager.Attach(formsPlot);

// Add shapes
var line = formsPlot.Plot.Add.Line(0, 0, 10, 10);
var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);
shapeManager.AddShape(drawnShape);

// Select shape programmatically
shapeManager.ToggleSelection(drawnShape);
Console.WriteLine($"Is selected: {drawnShape.IsSelected}"); // True

// Get selected shapes
var selected = shapeManager.SelectedShapes;
Console.WriteLine($"Selected count: {selected.Count}"); // 1

// Clear selection
shapeManager.ClearSelection();
Console.WriteLine($"Is selected: {drawnShape.IsSelected}"); // False
```

### Selection in Event Handlers

```csharp
// In ChartInteractions or custom mouse handler
private void OnMouseDown(object? sender, MouseEventArgs e)
{
    if (currentDrawMode == ChartDrawMode.None && e.Button == MouseButtons.Left)
    {
        // Check for Ctrl key
        bool addToSelection = (Control.ModifierKeys & Keys.Control) == Keys.Control;
        
        // Select shape at click position
        var selected = shapeManager.SelectShapeAt(e.X, e.Y, addToSelection);
        
        if (selected != null)
        {
            formsPlot.Refresh();
            Console.WriteLine($"Selected {selected.DrawMode} shape");
        }
        else if (!addToSelection)
        {
            shapeManager.ClearSelection();
        }
    }
}
```

### Deleting Selected Shapes

```csharp
// Delete selected shapes with undo support
if (shapeManager.SelectedShapes.Count > 0)
{
    shapeManager.DeleteSelectedShapes();
    formsPlot.Refresh();
    
    // Can undo if needed
    if (shapeManager.CanUndo)
    {
        shapeManager.Undo(); // Restore deleted shapes
        formsPlot.Refresh();
    }
}
```

## Implementation Details

### Selection Algorithm

The selection algorithm uses a distance-based approach:

1. Convert pixel coordinates to data coordinates
2. Calculate distance from click point to each shape's bounding box center
3. Select the closest shape within a threshold (5% of visible range)
4. Update visual appearance of selected shapes

**Note:** The current implementation uses a simplified distance calculation. For production use, consider:
- Implementing shape-specific hit testing (e.g., point-to-line distance for lines)
- Using spatial indexing (R-tree or quadtree) for better performance with many shapes
- Adding configurable selection tolerance

### Visual Updates

Selected shapes are highlighted using reflection to modify their properties:
- `LineWidth`: Increased by 1.5x
- `LineColor`: Changed to yellow (ScottPlot.Colors.Yellow)

This approach works across different plottable types (lines, rectangles, circles, etc.) by dynamically checking for and modifying common properties.

### Metadata Management

The ShapeManager maintains a mapping between `IPlottable` objects and `DrawnShape` wrappers:

```csharp
private readonly Dictionary<IPlottable, DrawnShape> _plottableToShape = new();
```

This allows:
- Efficient lookup when undoing/redoing operations
- Preservation of metadata across undo/redo cycles
- Separation of ScottPlot rendering from application metadata

## Testing

The implementation includes comprehensive unit tests covering:

- Shape selection and deselection
- Multi-selection with Ctrl key
- Clear selection functionality
- Selected shapes enumeration
- Delete selected shapes with undo/redo
- Edge cases (empty selection, unmanaged shapes)

See `ShapeManagerTests.cs` for complete test coverage.

## Performance Considerations

- **Selection Search**: O(n) linear search through all shapes
- **Memory Overhead**: ~100 bytes per shape for metadata
- **Visual Updates**: Reflection-based property updates (relatively fast)
- **Future Optimization**: Consider spatial indexing for large datasets (>1000 shapes)

## Future Enhancements

Potential improvements for future versions:

1. **Shape-Specific Hit Testing**: Implement accurate hit testing for each shape type
2. **Spatial Indexing**: Use R-tree or quadtree for O(log n) selection performance
3. **Selection Handles**: Add visual handles for resizing/moving selected shapes
4. **Selection Box**: Implement drag-to-select rectangle for multi-selection
5. **Customizable Visuals**: Allow configuration of selection colors and styles
6. **Shape Properties Panel**: UI for editing selected shape properties
7. **Grouping**: Allow shapes to be grouped and selected as a unit
8. **Layer Support**: Organize shapes into layers with independent selection

## Breaking Changes

### Migration from Previous Version

The following changes were made to the API:

**IShapeManager:**
- `AddShape()` now takes `DrawnShape` instead of `IPlottable`
- `DeleteShape()` now takes `DrawnShape` instead of `IPlottable`
- `Shapes` property now returns `IReadOnlyList<DrawnShape>` instead of `IReadOnlyList<IPlottable>`

**Migration Example:**
```csharp
// Before
var line = formsPlot.Plot.Add.Line(0, 0, 10, 10);
shapeManager.AddShape(line);

// After
var line = formsPlot.Plot.Add.Line(0, 0, 10, 10);
var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);
shapeManager.AddShape(drawnShape);
```

### Backward Compatibility

The underlying `IPlottable` objects are still accessible via `DrawnShape.Plottable`, allowing:
- Direct manipulation of ScottPlot properties if needed
- Integration with existing ScottPlot code
- Custom rendering or styling

## Support and Troubleshooting

### Common Issues

**Q: Selection doesn't work for my shape type**

A: Ensure your shape implements `GetAxisLimits()` method correctly. The selection algorithm relies on this to calculate the shape's bounding box.

**Q: Selected shapes don't show visual feedback**

A: Check that your shape has `LineWidth` and `LineColor` properties. Some custom plottables may use different property names.

**Q: Selection is too sensitive/not sensitive enough**

A: Adjust the selection threshold in `ShapeManager.SelectShapeAt()`. The default is 5% of visible range.

### Debug Tips

Enable diagnostic logging:
```csharp
var selected = shapeManager.SelectShapeAt(x, y, false);
if (selected == null)
{
    Console.WriteLine($"No shape found at ({x}, {y})");
    Console.WriteLine($"Total shapes: {shapeManager.Shapes.Count}");
    Console.WriteLine($"Visible shapes: {shapeManager.Shapes.Count(s => s.IsVisible)}");
}
```

## Conclusion

The shape selection API provides a robust foundation for interactive chart applications. The metadata-based approach using `DrawnShape` wrappers enables rich functionality while maintaining clean separation between rendering and application logic.

For questions or issues, please refer to the repository's issue tracker or contact the development team.
