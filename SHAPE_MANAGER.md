# ShapeManager Documentation

## Overview

The ShapeManager is a service that manages all drawn shapes on the chart with support for undo/redo operations using the Command pattern. It provides centralized management of chart objects for future persistence and editing capabilities.

## Architecture

### Command Pattern

The implementation uses the Command pattern to enable reversible operations:

```
ICommand
├── AddShapeCommand - Adds a shape to the chart
└── DeleteShapeCommand - Removes a shape from the chart
```

Each command implements two methods:
- `Execute()`: Performs the operation
- `Undo()`: Reverts the operation

### ShapeManager

The ShapeManager maintains:
- **Shapes List**: All currently drawn shapes
- **Undo Stack**: History of executed commands
- **Redo Stack**: History of undone commands

## Usage

### Basic Operations

```csharp
// Get the shape manager (via DI or from ChartInteractions)
var shapeManager = _chartInteractions.ShapeManager;

// Add a shape
var line = formsPlot.Plot.Add.Line(0, 0, 10, 10);
shapeManager.AddShape(line);

// Undo the last operation
if (shapeManager.CanUndo)
{
    shapeManager.Undo();
}

// Redo the undone operation
if (shapeManager.CanRedo)
{
    shapeManager.Redo();
}

// Delete a shape
shapeManager.DeleteShape(line);

// Get all shapes
var allShapes = shapeManager.Shapes;
```

### Integration with ChartInteractions

The ChartInteractions service automatically uses the ShapeManager when finalizing shapes:

```csharp
// When a user draws a shape, it's automatically added via ShapeManager
// This means all drawn shapes are tracked and can be undone/redone
```

### Keyboard Shortcuts

The MainForm handles keyboard shortcuts:
- **Ctrl+Z**: Undo last operation
- **Ctrl+Y**: Redo last undone operation

## Implementation Details

### Command Execution Flow

1. User performs an action (e.g., draws a shape)
2. Action is wrapped in a Command object
3. Command is executed and added to the undo stack
4. Redo stack is cleared (new actions invalidate redo history)

### Undo/Redo Flow

**Undo:**
1. Pop command from undo stack
2. Call `command.Undo()`
3. Push command to redo stack
4. Update shapes list

**Redo:**
1. Pop command from redo stack
2. Call `command.Execute()`
3. Push command to undo stack
4. Update shapes list

## Benefits

### 1. Centralized Management
All shapes are tracked in one place, making it easy to:
- Query all shapes
- Save/load shapes
- Apply bulk operations

### 2. Extensibility
Adding new commands is straightforward:
```csharp
public class MoveShapeCommand : ICommand
{
    private readonly IPlottable _shape;
    private readonly Coordinates _oldPosition;
    private readonly Coordinates _newPosition;
    
    public void Execute() { /* Move shape to new position */ }
    public void Undo() { /* Restore old position */ }
}
```

### 3. User Experience
Users can experiment freely, knowing they can undo mistakes. This encourages exploration and learning.

### 4. Testability
The Command pattern makes testing straightforward:
- Test individual commands in isolation
- Test undo/redo stacks
- Mock dependencies easily

## Testing

The solution includes comprehensive unit tests:

### CommandTests
- `AddShapeCommand_Execute_AddsShapeToPlot`
- `AddShapeCommand_Undo_RemovesShapeFromPlot`
- `DeleteShapeCommand_Execute_RemovesShapeFromPlot`
- `DeleteShapeCommand_Undo_RestoresShapeToPlot`
- `AddShapeCommand_ExecuteAndUndoMultipleTimes`

### ShapeManagerTests
- `ShapeManager_CanAttach`
- `AddShape_AddsShapeToChart`
- `AddShape_EnablesUndo`
- `Undo_RemovesLastShape`
- `Redo_RestoresUndoneShape`
- `AddShape_ClearsRedoStack`
- `MultipleShapes_UndoRedoSequence`
- `DeleteShape_RemovesShapeFromChart`
- `DeleteShape_UndoRestoresShape`

Run tests with:
```bash
dotnet test ChartPro.Tests/ChartPro.Tests.csproj
```

**Note**: Tests require Windows due to WinForms dependencies.

## Future Enhancements

### Selection and Editing
- Select shapes with mouse click
- Drag to move shapes
- Resize handles for shapes
- Delete key to remove selected shape

### Command Extensions
- Move command
- Resize command
- Edit properties command (color, style, etc.)
- Batch operations (move multiple, delete multiple)

### Persistence
- Save shapes to file
- Load shapes from file
- Export shapes to different formats

### History Management
- Set maximum undo depth
- Clear history command
- Undo/redo hints in UI

## Dependencies

- ScottPlot 5.0.47
- ScottPlot.WinForms 5.0.47
- .NET 8.0

## API Reference

### IShapeManager Interface

```csharp
public interface IShapeManager : IDisposable
{
    void Attach(FormsPlot formsPlot);
    void AddShape(IPlottable shape);
    void DeleteShape(IPlottable shape);
    bool Undo();
    bool Redo();
    IReadOnlyList<IPlottable> Shapes { get; }
    bool CanUndo { get; }
    bool CanRedo { get; }
    bool IsAttached { get; }
}
```

### ICommand Interface

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
}
```

## Best Practices

1. **Always use ShapeManager for shape operations**: Don't bypass it by adding shapes directly to the plot
2. **Check CanUndo/CanRedo before calling**: Prevents unnecessary operations
3. **Dispose properly**: ShapeManager implements IDisposable - always dispose when done
4. **Use DI**: Register ShapeManager as a service and inject where needed
5. **Test commands**: Write unit tests for any new command implementations

## Troubleshooting

### Undo/Redo not working
- Ensure KeyPreview is enabled on the form
- Check that keyboard event handlers are attached
- Verify ShapeManager is properly initialized

### Shapes not tracked
- Ensure shapes are added via ShapeManager.AddShape()
- Check that ShapeManager is attached before use

### Memory leaks
- Always call Dispose() on ShapeManager
- Verify event handlers are unhooked in Dispose()
