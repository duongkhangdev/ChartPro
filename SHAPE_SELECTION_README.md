# Shape Selection Feature - README

## Quick Reference

This feature enables interactive selection and manipulation of drawn shapes on the chart.

## 🎯 What's New

- ✅ Click on shapes to select them
- ✅ Multi-select shapes with Ctrl+Click
- ✅ Visual feedback with highlighted selection
- ✅ Delete selected shapes with undo/redo support
- ✅ Comprehensive API for programmatic shape manipulation

## 📚 Documentation

- **[API Reference](SHAPE_SELECTION_API.md)** - Complete API documentation
- **[Usage Guide](SHAPE_SELECTION_USAGE_GUIDE.md)** - Quick start and examples
- **[Implementation Summary](SHAPE_SELECTION_IMPLEMENTATION_SUMMARY.md)** - Technical details

## 🚀 Quick Start

### Basic Selection

```csharp
// User clicks on a shape (when not in drawing mode)
// Shape automatically gets selected and highlighted

// Check selected shapes
var selectedCount = shapeManager.SelectedShapes.Count;
Console.WriteLine($"Selected: {selectedCount} shapes");
```

### Delete Selected

```csharp
// Delete selected shapes (with undo support)
chartInteractions.DeleteSelectedShapes();

// Undo if needed
if (chartInteractions.ShapeManager.CanUndo)
{
    chartInteractions.ShapeManager.Undo();
}
```

### Programmatic Selection

```csharp
// Select a shape programmatically
shapeManager.ToggleSelection(myShape);

// Clear all selection
shapeManager.ClearSelection();

// Get all selected shapes
foreach (var shape in shapeManager.SelectedShapes)
{
    Console.WriteLine($"Selected: {shape.DrawMode} - {shape.Id}");
}
```

## 🔑 Key Features

### 1. Single Selection
Click on any shape to select it (deselects others)

### 2. Multi-Selection
Hold `Ctrl` and click to add shapes to selection

### 3. Visual Feedback
- Selected shapes are highlighted in **yellow**
- Line width increased by **1.5x**
- Instant visual response

### 4. Metadata Tracking
Each shape has:
- Unique identifier (Guid)
- Selection state (bool)
- Drawing mode (ChartDrawMode)
- Visibility state (bool)
- Creation timestamp (DateTime)

### 5. Undo/Redo Support
All shape operations, including deletion, support undo/redo

## 🛠️ Integration

### Mouse Event Handling

The feature integrates seamlessly with existing mouse event handling:

```csharp
private void OnMouseDown(MouseEventArgs e)
{
    if (currentDrawMode == ChartDrawMode.None)
    {
        // Selection mode
        bool addToSelection = (Control.ModifierKeys & Keys.Control) == Keys.Control;
        var selected = shapeManager.SelectShapeAt(e.X, e.Y, addToSelection);
        
        if (selected != null)
        {
            formsPlot.Refresh();
        }
    }
    else
    {
        // Drawing mode - create shapes
    }
}
```

### Keyboard Shortcuts (Example)

```csharp
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    switch (keyData)
    {
        case Keys.Delete:
            chartInteractions.DeleteSelectedShapes();
            return true;
            
        case Keys.Escape:
            chartInteractions.ShapeManager.ClearSelection();
            return true;
            
        case Keys.Control | Keys.A:
            // Select all shapes
            foreach (var shape in chartInteractions.ShapeManager.Shapes)
                shape.IsSelected = true;
            formsPlot.Refresh();
            return true;
    }
    return base.ProcessCmdKey(ref msg, keyData);
}
```

## 📊 API Overview

### IShapeManager Interface

```csharp
// Shape management
void AddShape(DrawnShape shape);
void DeleteShape(DrawnShape shape);
IReadOnlyList<DrawnShape> Shapes { get; }

// Selection
DrawnShape? SelectShapeAt(int pixelX, int pixelY, bool addToSelection = false);
void ToggleSelection(DrawnShape shape);
void ClearSelection();
IReadOnlyList<DrawnShape> SelectedShapes { get; }
void DeleteSelectedShapes();

// Undo/Redo
bool Undo();
bool Redo();
bool CanUndo { get; }
bool CanRedo { get; }
```

### DrawnShape Class

```csharp
public class DrawnShape
{
    public Guid Id { get; }
    public IPlottable Plottable { get; }
    public ChartDrawMode DrawMode { get; }
    public bool IsSelected { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; }
}
```

## ⚡ Performance

- **Selection**: O(n) linear search through all shapes
- **Memory**: ~100 bytes per shape for metadata
- **Visual Updates**: Fast reflection-based property updates
- **Recommended**: Works well up to ~1000 shapes

## 🧪 Testing

Comprehensive unit tests cover:
- ✅ Shape addition and deletion with DrawnShape
- ✅ Selection toggling
- ✅ Multi-selection
- ✅ Clear selection
- ✅ Delete selected shapes
- ✅ Undo/Redo with selection
- ✅ Edge cases and error handling

Run tests:
```bash
dotnet test ChartPro.Tests
```

## 🔄 Migration Guide

### Before (Old API)
```csharp
var line = plot.Add.Line(0, 0, 10, 10);
shapeManager.AddShape(line);  // IPlottable
```

### After (New API)
```csharp
var line = plot.Add.Line(0, 0, 10, 10);
var drawnShape = new DrawnShape(line, ChartDrawMode.TrendLine);
shapeManager.AddShape(drawnShape);  // DrawnShape

// Access underlying plottable if needed
var plottable = drawnShape.Plottable;
```

## 🎨 Customization

The implementation uses reflection to update visual properties, making it easy to customize:

```csharp
// In ShapeManager.UpdateShapeVisual()
// Modify these values to customize selection appearance:
- Line width multiplier: 1.5x (default)
- Highlight color: Yellow (default)
```

Future versions will allow configuration through a `SelectionStyle` class.

## 📝 Common Scenarios

### Scenario 1: Context Menu for Selected Shapes
```csharp
private void ShowContextMenu(Point location)
{
    if (shapeManager.SelectedShapes.Count > 0)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add($"Delete {shapeManager.SelectedShapes.Count} shape(s)", 
            null, (s, e) => chartInteractions.DeleteSelectedShapes());
        menu.Items.Add("Clear Selection", 
            null, (s, e) => shapeManager.ClearSelection());
        menu.Show(formsPlot, location);
    }
}
```

### Scenario 2: Status Bar Update
```csharp
private void UpdateStatusBar()
{
    var total = shapeManager.Shapes.Count;
    var selected = shapeManager.SelectedShapes.Count;
    statusLabel.Text = $"Shapes: {total} | Selected: {selected}";
}
```

### Scenario 3: Toolbar Buttons
```csharp
deleteButton.Enabled = shapeManager.SelectedShapes.Count > 0;
undoButton.Enabled = shapeManager.CanUndo;
redoButton.Enabled = shapeManager.CanRedo;
```

## 🚧 Known Limitations

1. **Selection Accuracy**: Uses bounding box center distance (future: implement shape-specific hit testing)
2. **Visual Customization**: Currently hardcoded (future: configurable SelectionStyle)
3. **Performance**: O(n) selection search (future: spatial indexing for >1000 shapes)

## 🔮 Future Enhancements

Planned improvements:
- [ ] Shape-specific hit testing for better accuracy
- [ ] Configurable selection visuals
- [ ] Selection changed events
- [ ] Drag-to-select rectangle
- [ ] Selection handles for resizing/moving
- [ ] Shape grouping
- [ ] Layer management

## 💡 Tips

1. Always call `formsPlot.Refresh()` after selection changes
2. Check `CanUndo`/`CanRedo` before enabling buttons
3. Use `IsSelected` property for custom logic
4. Consider performance with >1000 shapes
5. Combine with keyboard shortcuts for better UX

## 🐛 Troubleshooting

**Q: Selection doesn't work**
- Ensure no drawing mode is active
- Check that shapes were added through ShapeManager
- Verify click is within chart bounds

**Q: No visual feedback**
- Call `formsPlot.Refresh()` after selection
- Check shape has `LineWidth` and `LineColor` properties
- Ensure shape is visible (`IsVisible = true`)

**Q: Can't select small shapes**
- Adjust selection threshold in `SelectShapeAt()`
- Consider implementing shape-specific hit testing

## 📞 Support

For issues or questions:
1. Check the [API Reference](SHAPE_SELECTION_API.md)
2. Review the [Usage Guide](SHAPE_SELECTION_USAGE_GUIDE.md)
3. Check [Implementation Summary](SHAPE_SELECTION_IMPLEMENTATION_SUMMARY.md)
4. Open an issue on GitHub
5. Contact the development team

## 📄 License

Same as ChartPro project license.

---

**Version**: 1.0.0  
**Status**: ✅ Production Ready  
**Last Updated**: 2024

Happy charting! 📊✨
