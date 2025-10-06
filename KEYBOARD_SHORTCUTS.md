# ChartPro Keyboard Shortcuts

## Quick Reference

| Key | Action | Description |
|-----|--------|-------------|
| `ESC` | Cancel Drawing | Returns to None mode and enables pan/zoom |
| `1` | Trend Line | Activates trend line drawing tool |
| `2` | Horizontal Line | Activates horizontal line drawing tool |
| `3` | Vertical Line | Activates vertical line drawing tool |
| `4` | Rectangle | Activates rectangle drawing tool |
| `5` | Circle | Activates circle drawing tool |
| `6` | Fibonacci | Activates Fibonacci retracement tool |
| `Ctrl+Z` | Undo | Undoes the last drawing operation |
| `Ctrl+Y` or `Ctrl+Shift+Z` | Redo | Redoes the last undone operation |
| `Delete` | Delete Selected | Deletes currently selected shapes |

## Usage

### Quick Tool Selection
Press the number keys (1-6) at any time to switch between drawing tools. The toolbar button will highlight automatically, and the status bar will update to show the current mode.

### Cancel Drawing
Press `ESC` at any time to:
- Cancel the current drawing operation
- Return to "None" mode
- Re-enable pan and zoom functionality
- Clear any preview shapes

### During Drawing
While drawing a shape:
1. The status bar shows real-time coordinates
2. Shape parameters are displayed (length, angle, dimensions)
3. Press `ESC` to cancel the current drawing

## Status Bar Information

The status bar at the bottom of the window displays three types of information:

### 1. Mode Display
Shows the current drawing mode:
- "Mode: None" - Pan/zoom enabled
- "Mode: Trend Line" - Drawing trend lines
- "Mode: Horizontal Line" - Drawing horizontal lines
- "Mode: Vertical Line" - Drawing vertical lines
- "Mode: Rectangle" - Drawing rectangles
- "Mode: Circle" - Drawing circles
- "Mode: Fibonacci" - Drawing Fibonacci retracement

### 2. Coordinates
Shows the current mouse position on the chart:
- Format: "X: 123.45, Y: 67.89"
- X coordinate represents time/date
- Y coordinate represents price
- Updates in real-time as you move the mouse

### 3. Shape Information
Displays parameters of the shape being drawn:

**Trend Line:**
- Length: Total distance between start and end points
- Angle: Angle of the line in degrees

**Horizontal Line:**
- Price: Y-coordinate (price level) of the line

**Vertical Line:**
- Time: X-coordinate (time position) of the line

**Rectangle:**
- Width: Horizontal distance
- Height: Vertical distance

**Circle:**
- RadiusX: Half of the horizontal distance
- RadiusY: Half of the vertical distance

**Fibonacci:**
- Range: Vertical distance covered

## Tooltips

Hover over any toolbar button to see a tooltip with:
- Brief description of the tool
- Keyboard shortcut in parentheses

## Undo/Redo Operations

### Undo
Press `Ctrl+Z` to:
- Undo the last drawing operation
- Remove the most recently added shape
- Works recursively - press multiple times to undo multiple operations
- Unavailable when there are no operations to undo

### Redo
Press `Ctrl+Y` or `Ctrl+Shift+Z` to:
- Redo the last undone operation
- Restore the most recently removed shape
- Works recursively - press multiple times to redo multiple operations
- Unavailable when there are no operations to redo
- Redo history is cleared when a new shape is drawn

### Delete Selected
Press `Delete` to:
- Delete currently selected shapes from the chart
- Note: Shape selection feature is planned for future implementation

## Future Enhancements

Planned features:
- Shape selection and editing
- Save/Load annotations to file
- Additional keyboard shortcuts for advanced operations

## Tips

1. **Efficiency**: Use keyboard shortcuts to switch tools quickly without reaching for the mouse
2. **Workflow**: Use `ESC` to return to pan/zoom mode after drawing each shape
3. **Precision**: Watch the status bar for exact coordinates while drawing
4. **Learning**: Keyboard shortcuts are shown in button text for easy reference
