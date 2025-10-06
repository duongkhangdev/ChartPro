# Draw Mode Strategy Pattern Architecture

## Overview

The ChartInteractions service uses the **Strategy Pattern** combined with a **Factory Pattern** to manage different drawing modes. This architecture was introduced to improve extensibility, reduce complexity, and enable easier testing and maintenance.

## Architecture Components

### 1. IDrawModeStrategy Interface

The core interface that all drawing mode strategies must implement:

```csharp
public interface IDrawModeStrategy
{
    /// <summary>
    /// Creates a preview plottable for the current drawing operation.
    /// </summary>
    IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot);

    /// <summary>
    /// Creates the final plottable after drawing is complete.
    /// </summary>
    IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot);
}
```

**Responsibilities:**
- Define contract for preview creation (shown during mouse drag)
- Define contract for final shape creation (shown after mouse release)
- Encapsulate all drawing logic for a specific mode

### 2. Strategy Implementations

Each drawing mode has its own strategy implementation:

#### TrendLineStrategy
- **Preview**: Gray semi-transparent line (1px width)
- **Final**: Blue solid line (2px width)
- **Behavior**: Draws a line from start to end coordinates

#### HorizontalLineStrategy
- **Preview**: Gray semi-transparent horizontal line (1px width)
- **Final**: Green solid horizontal line (2px width)
- **Behavior**: Draws a horizontal line at the Y coordinate of the end point

#### VerticalLineStrategy
- **Preview**: Gray semi-transparent vertical line (1px width)
- **Final**: Orange solid vertical line (2px width)
- **Behavior**: Draws a vertical line at the X coordinate of the end point

#### RectangleStrategy
- **Preview**: Gray semi-transparent rectangle (1px border, light fill)
- **Final**: Purple solid rectangle (2px border, light fill)
- **Behavior**: Draws a rectangle using min/max of start and end coordinates

#### CircleStrategy
- **Preview**: Gray semi-transparent ellipse (1px border, light fill)
- **Final**: Cyan solid ellipse (2px border, light fill)
- **Behavior**: Draws an ellipse centered between start and end points

#### FibonacciRetracementStrategy
- **Preview**: Gold semi-transparent line (1px width)
- **Final**: Gold solid line (2px width)
- **Behavior**: Currently draws a simple line; can be extended to include retracement levels

### 3. DrawModeStrategyFactory

Factory class responsible for creating strategy instances:

```csharp
public static class DrawModeStrategyFactory
{
    public static IDrawModeStrategy? CreateStrategy(ChartDrawMode mode)
    {
        return mode switch
        {
            ChartDrawMode.TrendLine => new TrendLineStrategy(),
            ChartDrawMode.HorizontalLine => new HorizontalLineStrategy(),
            ChartDrawMode.VerticalLine => new VerticalLineStrategy(),
            ChartDrawMode.Rectangle => new RectangleStrategy(),
            ChartDrawMode.Circle => new CircleStrategy(),
            ChartDrawMode.FibonacciRetracement => new FibonacciRetracementStrategy(),
            _ => null
        };
    }
}
```

**Responsibilities:**
- Map `ChartDrawMode` enum values to strategy instances
- Return `null` for unsupported or `None` mode
- Centralize strategy instantiation logic

### 4. ChartInteractions Service

The refactored service now uses strategies instead of switch-case statements:

**Before (Switch-Case):**
```csharp
private void UpdatePreview(Coordinates start, Coordinates end)
{
    _previewPlottable = _currentDrawMode switch
    {
        ChartDrawMode.TrendLine => CreateTrendLinePreview(start, end),
        ChartDrawMode.HorizontalLine => CreateHorizontalLinePreview(start, end),
        // ... many more cases
        _ => null
    };
}
```

**After (Strategy Pattern):**
```csharp
private void UpdatePreview(Coordinates start, Coordinates end)
{
    var strategy = DrawModeStrategyFactory.CreateStrategy(_currentDrawMode);
    if (strategy != null)
    {
        _previewPlottable = strategy.CreatePreview(start, end, _formsPlot.Plot);
        _formsPlot.Plot.Add.Plottable(_previewPlottable);
        _formsPlot.Refresh();
    }
}
```

## Benefits

### 1. Improved Extensibility
- **Adding new draw modes**: Simply create a new strategy class implementing `IDrawModeStrategy`
- **No modifications needed**: ChartInteractions service doesn't need to change
- **Factory updates only**: Only update the factory's switch statement

### 2. Reduced Complexity
- **Separation of concerns**: Each strategy is self-contained
- **Smaller methods**: No more large switch statements
- **Single responsibility**: Each class has one job

### 3. Easier Testing
- **Unit testable**: Each strategy can be tested independently
- **Mockable**: Strategies can be mocked for integration tests
- **Clear boundaries**: Test one strategy at a time

### 4. Better Maintainability
- **Isolated changes**: Changes to one mode don't affect others
- **Clear structure**: Easy to find and modify specific behavior
- **Self-documenting**: Strategy names clearly indicate purpose

## Adding a New Draw Mode

To add a new drawing mode, follow these steps:

### Step 1: Add to ChartDrawMode Enum
```csharp
public enum ChartDrawMode
{
    // ... existing modes
    YourNewMode
}
```

### Step 2: Create Strategy Class
```csharp
public class YourNewModeStrategy : IDrawModeStrategy
{
    public IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot)
    {
        // Create preview plottable
        // Use gray color with alpha for preview
        // Use 1px line width
    }

    public IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot)
    {
        // Create final plottable
        // Use appropriate color
        // Use 2px line width
    }
}
```

### Step 3: Update Factory
```csharp
public static IDrawModeStrategy? CreateStrategy(ChartDrawMode mode)
{
    return mode switch
    {
        // ... existing cases
        ChartDrawMode.YourNewMode => new YourNewModeStrategy(),
        _ => null
    };
}
```

### Step 4: Add Unit Tests
```csharp
public class YourNewModeStrategyTests
{
    [Fact]
    public void CreatePreview_ShouldReturnNotNull()
    {
        // Test preview creation
    }

    [Fact]
    public void CreateFinal_ShouldReturnNotNull()
    {
        // Test final shape creation
    }
}
```

## Design Patterns Used

### Strategy Pattern
- **Purpose**: Define a family of algorithms, encapsulate each one, and make them interchangeable
- **Usage**: Each draw mode is a separate strategy
- **Benefit**: Algorithm can vary independently from clients that use it

### Factory Pattern
- **Purpose**: Provide an interface for creating objects without specifying their concrete classes
- **Usage**: DrawModeStrategyFactory creates strategy instances
- **Benefit**: Centralized object creation logic

## Future Enhancements

### Potential Improvements

1. **Strategy Configuration**
   - Allow strategies to be configured (colors, line widths, etc.)
   - Support user preferences for draw styles

2. **Multi-Step Strategies**
   - Support strategies that require more than two points
   - Extend interface with `OnMouseDown`, `OnMouseMove`, `OnMouseUp` methods

3. **Strategy Composition**
   - Combine multiple strategies (e.g., trend line with text label)
   - Decorator pattern for adding behavior

4. **Strategy Persistence**
   - Save/load strategy settings
   - Export/import drawing templates

5. **Interactive Editing**
   - Allow editing of drawn shapes
   - Move, resize, delete operations

## Testing Strategy

### Unit Tests
- Test each strategy independently
- Verify preview and final methods return valid plottables
- Test edge cases (same start/end points, reversed coordinates)

### Integration Tests
- Test factory creates correct strategy for each mode
- Test ChartInteractions uses strategies correctly
- Test full drawing workflow (mouse down → move → up)

### Test Location
All strategy tests are located in:
```
ChartPro.Tests/Strategies/
├── TrendLineStrategyTests.cs
├── HorizontalLineStrategyTests.cs
├── VerticalLineStrategyTests.cs
├── RectangleStrategyTests.cs
├── CircleStrategyTests.cs
├── FibonacciRetracementStrategyTests.cs
└── DrawModeStrategyFactoryTests.cs
```

## Performance Considerations

### Strategy Creation
- Strategies are created on-demand during drawing operations
- Lightweight objects with minimal overhead
- No caching needed due to low creation cost

### Memory Usage
- Strategies don't maintain state
- No memory leaks from strategy instances
- Preview plottables are properly cleaned up

### Rendering
- No change to rendering performance
- Same ScottPlot plottables used as before
- Efficient mouse event handling

## Related Documentation

- [IMPLEMENTATION.md](IMPLEMENTATION.md) - Original implementation details
- [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md) - Development guidelines
- [README.md](README.md) - Project overview

## Summary

The strategy pattern refactoring successfully:
- ✅ Removed complex switch-case statements
- ✅ Isolated drawing logic into separate classes
- ✅ Made the system extensible for new draw modes
- ✅ Enabled comprehensive unit testing
- ✅ Improved code maintainability and readability

This architecture provides a solid foundation for future enhancements while maintaining backward compatibility with existing functionality.
