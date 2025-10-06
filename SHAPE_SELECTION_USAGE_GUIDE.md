# Shape Selection - Quick Start Guide

This guide provides a quick introduction to using the shape selection feature in ChartPro.

## Basic Concepts

- **DrawnShape**: A wrapper around ScottPlot's `IPlottable` that adds metadata like selection state, ID, and draw mode
- **Selection**: Shapes can be selected by clicking on them, allowing operations like deletion or future editing
- **Visual Feedback**: Selected shapes are highlighted with increased line width and yellow color

## How to Use

### 1. Drawing Shapes

Shapes are automatically wrapped in `DrawnShape` when created through the drawing tools:

```csharp
// User draws a line using the UI
// Internally, ChartInteractions creates:
var plottable = strategy.CreateFinal(start, end, plot);
var drawnShape = new DrawnShape(plottable, ChartDrawMode.TrendLine);
shapeManager.AddShape(drawnShape);
```

### 2. Selecting Shapes

**Single Selection:**
1. Ensure no drawing mode is active
2. Click on a shape to select it
3. The shape will be highlighted

**Multi-Selection:**
1. Hold `Ctrl` key
2. Click on multiple shapes to add them to selection
3. All selected shapes will be highlighted

**Clear Selection:**
1. Click on empty chart area (without Ctrl)
2. All shapes will be deselected

### 3. Deleting Selected Shapes

```csharp
// In your UI button click handler or keyboard shortcut
if (chartInteractions.ShapeManager.SelectedShapes.Count > 0)
{
    chartInteractions.DeleteSelectedShapes();
}
```

### 4. Working with Selected Shapes Programmatically

```csharp
// Get all selected shapes
var selected = shapeManager.SelectedShapes;

// Iterate through selected shapes
foreach (var shape in selected)
{
    Console.WriteLine($"Shape ID: {shape.Id}");
    Console.WriteLine($"Type: {shape.DrawMode}");
    Console.WriteLine($"Created: {shape.CreatedAt}");
}

// Check if specific shape is selected
if (myShape.IsSelected)
{
    // Do something with selected shape
}
```

### 5. Undo/Redo with Selection

Deletion of selected shapes supports undo/redo:

```csharp
// Delete shapes
shapeManager.DeleteSelectedShapes();

// Undo deletion (shapes reappear)
shapeManager.Undo();

// Redo deletion
shapeManager.Redo();
```

## Common Scenarios

### Scenario 1: Delete Selected Shapes on Delete Key

Add keyboard handler to your form:

```csharp
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    if (keyData == Keys.Delete)
    {
        if (_chartInteractions.ShapeManager.SelectedShapes.Count > 0)
        {
            _chartInteractions.DeleteSelectedShapes();
            return true;
        }
    }
    return base.ProcessCmdKey(ref msg, keyData);
}
```

### Scenario 2: Show Selection Count

```csharp
private void UpdateStatusBar()
{
    var selectedCount = _chartInteractions.ShapeManager.SelectedShapes.Count;
    var totalCount = _chartInteractions.ShapeManager.Shapes.Count;
    statusLabel.Text = $"Selected: {selectedCount} / {totalCount} shapes";
}
```

### Scenario 3: Clear Selection Programmatically

```csharp
private void ClearSelectionButton_Click(object sender, EventArgs e)
{
    _chartInteractions.ShapeManager.ClearSelection();
    formsPlot.Refresh();
}
```

### Scenario 4: Select All Shapes

```csharp
private void SelectAllShapes()
{
    foreach (var shape in _chartInteractions.ShapeManager.Shapes)
    {
        shape.IsSelected = true;
        // Update visual would normally be called internally
    }
    formsPlot.Refresh();
}
```

### Scenario 5: Toggle Shape Selection

```csharp
private void ToggleShapeSelection(DrawnShape shape)
{
    _chartInteractions.ShapeManager.ToggleSelection(shape);
}
```

## UI Integration Examples

### Add Context Menu for Selected Shapes

```csharp
private void formsPlot_MouseClick(object sender, MouseEventArgs e)
{
    if (e.Button == MouseButtons.Right)
    {
        var selected = _chartInteractions.ShapeManager.SelectedShapes;
        if (selected.Count > 0)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Delete Selected", null, (s, args) => 
            {
                _chartInteractions.DeleteSelectedShapes();
            });
            menu.Items.Add("Clear Selection", null, (s, args) => 
            {
                _chartInteractions.ShapeManager.ClearSelection();
            });
            menu.Show(formsPlot, e.Location);
        }
    }
}
```

### Add Toolbar Buttons

```csharp
// In your form's designer or constructor
var deleteButton = new ToolStripButton("Delete Selected");
deleteButton.Click += (s, e) => 
{
    if (_chartInteractions.ShapeManager.SelectedShapes.Count > 0)
    {
        _chartInteractions.DeleteSelectedShapes();
    }
};

var clearButton = new ToolStripButton("Clear Selection");
clearButton.Click += (s, e) => 
{
    _chartInteractions.ShapeManager.ClearSelection();
};

toolbar.Items.Add(deleteButton);
toolbar.Items.Add(clearButton);
```

## Tips and Best Practices

1. **Always Check for Null**: When selecting shapes, check if the returned shape is null
2. **Refresh After Changes**: Call `formsPlot.Refresh()` after selection changes to update the display
3. **Use CanUndo/CanRedo**: Check these properties before enabling undo/redo buttons
4. **Handle Empty Selection**: Provide feedback when no shapes are selected and user tries to delete
5. **Consider Performance**: For large numbers of shapes (>1000), consider optimizations

## Troubleshooting

**Problem**: Selection doesn't work

**Solutions**:
- Ensure no drawing mode is active (`ChartDrawMode.None`)
- Check that shapes have been added through `ShapeManager.AddShape()`
- Verify the click coordinates are within the chart bounds

**Problem**: Visual feedback not showing

**Solutions**:
- Call `formsPlot.Refresh()` after selection changes
- Check that the shape's plottable has `LineWidth` and `LineColor` properties
- Ensure the shape is not hidden (`IsVisible` should be true)

**Problem**: Can't select small shapes

**Solutions**:
- Adjust the selection threshold in `ShapeManager.SelectShapeAt()`
- Consider implementing shape-specific hit testing for better accuracy

## What's Next?

After mastering basic selection, you might want to explore:

- Moving selected shapes (future feature)
- Resizing selected shapes (future feature)
- Grouping shapes (future feature)
- Custom selection styles
- Batch operations on multiple selected shapes

## Related Documentation

- `SHAPE_SELECTION_API.md` - Complete API reference
- `SHAPE_MANAGER_IMPLEMENTATION.md` - Implementation details
- `IMPLEMENTATION.md` - Overall system architecture

## Need Help?

If you encounter issues or have questions:
1. Check the API documentation
2. Review the unit tests in `ShapeManagerTests.cs`
3. Open an issue on the GitHub repository
4. Contact the development team

---

Happy charting! 📊
