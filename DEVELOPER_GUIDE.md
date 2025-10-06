# ChartPro Developer Guide

## Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (recommended) or VS Code
- Windows OS (for running WinForms)

### Build and Run

```bash
# Clone the repository
git clone https://github.com/duongkhangdev/ChartPro.git
cd ChartPro

# Restore packages
dotnet restore ChartPro/ChartPro.csproj

# Build
dotnet build ChartPro/ChartPro.csproj --configuration Release

# Run
dotnet run --project ChartPro/ChartPro.csproj
```

## Using the Chart Interactions Service

### Basic Usage

```csharp
// In your form constructor (DI injected)
public MainForm(IChartInteractions chartInteractions)
{
    _chartInteractions = chartInteractions;
}

// In Form_Load
private void Form_Load(object sender, EventArgs e)
{
    // Attach to your FormsPlot control
    _chartInteractions.Attach(formsPlot1, pricePlotIndex: 0);
    _chartInteractions.EnableAll();
}

// Always dispose when done
private void Form_FormClosing(object sender, FormClosingEventArgs e)
{
    _chartInteractions.Dispose();
}
```

### Setting Draw Modes

```csharp
// Change drawing mode
_chartInteractions.SetDrawMode(ChartDrawMode.TrendLine);

// Available modes:
// - None (pan/zoom enabled)
// - TrendLine
// - HorizontalLine
// - VerticalLine
// - Rectangle
// - Circle
// - FibonacciRetracement
// - FibonacciExtension
// - Channel
// - Triangle
// - Text
```

### Working with Real-Time Data

```csharp
// 1. Create your candle list
var candles = new List<OHLC>();

// 2. Bind to the service
_chartInteractions.BindCandles(candles);

// 3. Add candlestick plot
formsPlot.Plot.Add.Candlestick(candles);

// 4. Update last candle (for live updates)
var updatedCandle = new OHLC(open, high, low, close, dateTime, timeSpan);
_chartInteractions.UpdateLastCandle(updatedCandle);

// 5. Add new candle (when time period changes)
var newCandle = new OHLC(open, high, low, close, dateTime, timeSpan);
_chartInteractions.AddCandle(newCandle);
```

## Extending the Service

### Adding New Drawing Tools (Using Strategy Pattern)

The ChartPro project uses the **Strategy Pattern** for drawing modes. This makes adding new draw modes straightforward and maintainable. For detailed architecture information, see [STRATEGY_PATTERN.md](STRATEGY_PATTERN.md).

#### Quick Steps:

1. **Add to ChartDrawMode enum** (ChartDrawMode.cs)
```csharp
public enum ChartDrawMode
{
    // ... existing modes
    MyNewTool
}
```

2. **Create Strategy Class** (ChartPro/Charting/Interactions/Strategies/MyNewToolStrategy.cs)
```csharp
using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

public class MyNewToolStrategy : IDrawModeStrategy
{
    public IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot)
    {
        // Create preview with gray, semi-transparent style
        var tool = plot.Add.MyNewTool(start, end);
        tool.LineWidth = 1;
        tool.LineColor = Colors.Gray.WithAlpha(0.5);
        return tool;
    }

    public IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot)
    {
        // Create final with colored, solid style
        var tool = plot.Add.MyNewTool(start, end);
        tool.LineWidth = 2;
        tool.LineColor = Colors.Blue;
        return tool;
    }
}
```

3. **Update Factory** (DrawModeStrategyFactory.cs)
```csharp
public static IDrawModeStrategy? CreateStrategy(ChartDrawMode mode)
{
    return mode switch
    {
        // ... existing cases
        ChartDrawMode.MyNewTool => new MyNewToolStrategy(),
        _ => null
    };
}
```

4. **Add Unit Tests** (ChartPro.Tests/Strategies/MyNewToolStrategyTests.cs)
```csharp
public class MyNewToolStrategyTests
{
    private readonly MyNewToolStrategy _strategy;
    private readonly Plot _plot;

    public MyNewToolStrategyTests()
    {
        _strategy = new MyNewToolStrategy();
        _plot = new Plot();
    }

    [Fact]
    public void CreatePreview_ShouldReturnNotNull()
    {
        var result = _strategy.CreatePreview(
            new Coordinates(10, 20), 
            new Coordinates(30, 40), 
            _plot);
        Assert.NotNull(result);
    }

    [Fact]
    public void CreateFinal_ShouldReturnNotNull()
    {
        var result = _strategy.CreateFinal(
            new Coordinates(10, 20), 
            new Coordinates(30, 40), 
            _plot);
        Assert.NotNull(result);
    }
}
```

5. **Add UI Button** (MainForm.cs)
```csharp
var btnMyNewTool = CreateToolButton("My New Tool", ChartDrawMode.MyNewTool, ref yPos);
toolbarPanel.Controls.Add(btnMyNewTool);
```

### Implementing Multi-Point Tools

For tools requiring more than 2 points (like triangles):

1. Extend state management:
```csharp
private List<Coordinates> _drawPoints = new();
private int _requiredPoints = 2; // or 3 for triangles
```

2. Modify mouse handlers:
```csharp
private void OnMouseUp(object? sender, MouseEventArgs e)
{
    if (_currentDrawMode == ChartDrawMode.None || _formsPlot == null)
        return;

    _drawPoints.Add(_formsPlot.Plot.GetCoordinates(e.X, e.Y));

    if (_drawPoints.Count >= _requiredPoints)
    {
        FinalizeMultiPointShape(_drawPoints);
        _drawPoints.Clear();
        SetDrawMode(ChartDrawMode.None);
    }
}
```

### Custom Event Handlers

You can extend the service to handle additional events:

```csharp
public interface IChartInteractions : IDisposable
{
    // Add new events
    event EventHandler<ShapeDrawnEventArgs>? ShapeDrawn;
    event EventHandler<DrawModeChangedEventArgs>? DrawModeChanged;
}
```

### Working with Snap/Magnet

The snap feature helps users draw more accurately by snapping coordinates to predefined points:

```csharp
// Enable snap functionality
_chartInteractions.SnapEnabled = true;

// Set snap mode
_chartInteractions.SnapMode = SnapMode.Price; // or SnapMode.CandleOHLC

// Snap can also be temporarily enabled by holding Shift key
```

**Snap Modes**:
- `SnapMode.None` - No snapping (default)
- `SnapMode.Price` - Snaps to rounded price levels with dynamic grid sizing
- `SnapMode.CandleOHLC` - Snaps to nearest candle's Open, High, Low, or Close values

**Implementation Details**:
- Snap logic is applied in mouse event handlers before creating shapes
- Price grid size is dynamically calculated based on visible range
- Candle snapping finds the nearest candle by time, then the closest OHLC value
- Shift key state is tracked via KeyDown/KeyUp events

## Architecture Patterns

### Dependency Injection

The service is registered as `Transient`:
```csharp
services.AddTransient<IChartInteractions, ChartInteractions>();
```

**Why Transient?**
- Each form gets its own instance
- Prevents state sharing between forms
- Proper lifecycle management

### Memory Management

The service implements `IDisposable` to ensure:
- Event handlers are unhooked
- References are cleared
- Memory leaks are prevented

Always call `Dispose()` when done:
```csharp
// Good
_chartInteractions.Dispose();

// Better (using statement)
using var interactions = serviceProvider.GetService<IChartInteractions>();
interactions.Attach(formsPlot);
// ... use it
// Automatically disposed when scope exits
```

### State Management

The service maintains:
- `_currentDrawMode`: Active drawing mode
- `_drawStartCoordinates`: Start point for shape
- `_previewPlottable`: Preview object during drawing
- `_boundCandles`: Reference to candle data
- `_isAttached`: Attachment state
- `_disposed`: Disposal state

## Testing

### Unit Testing

Create mock FormsPlot for testing:
```csharp
[Test]
public void SetDrawMode_DisablesInteraction_WhenNotNone()
{
    // Arrange
    var service = new ChartInteractions();
    var mockPlot = new Mock<FormsPlot>();
    service.Attach(mockPlot.Object);

    // Act
    service.SetDrawMode(ChartDrawMode.TrendLine);

    // Assert
    Assert.That(service.CurrentDrawMode, Is.EqualTo(ChartDrawMode.TrendLine));
    mockPlot.Verify(p => p.UserInputProcessor.IsEnabled = false);
}
```

### Integration Testing

Test with actual FormsPlot (requires Windows):
```csharp
[Test, RequiresSTA]
public void DrawTrendLine_CreatesLine_OnMouseUpDown()
{
    // Arrange
    var form = new Form();
    var plot = new FormsPlot { Parent = form };
    var service = new ChartInteractions();
    service.Attach(plot);
    service.SetDrawMode(ChartDrawMode.TrendLine);

    // Act
    // Simulate mouse events
    RaiseMouseDown(plot, 100, 100);
    RaiseMouseUp(plot, 200, 200);

    // Assert
    Assert.That(plot.Plot.GetPlottables().Count(), Is.GreaterThan(0));
}
```

## Debugging Tips

### Enable ScottPlot Debug Mode

```csharp
formsPlot.Configuration.DebugMode = true;
```

### Log Mouse Events

```csharp
private void OnMouseDown(object? sender, MouseEventArgs e)
{
    var coords = _formsPlot.Plot.GetCoordinates(e.X, e.Y);
    Debug.WriteLine($"MouseDown at ({coords.X}, {coords.Y})");
    // ... rest of implementation
}
```

### Inspect Plot State

```csharp
var plottables = formsPlot.Plot.GetPlottables();
Debug.WriteLine($"Total plottables: {plottables.Count()}");
foreach (var p in plottables)
{
    Debug.WriteLine($"  - {p.GetType().Name}");
}
```

## Performance Considerations

### Refresh Frequency

Avoid excessive refresh calls:
```csharp
// Bad
foreach (var candle in candles)
{
    _chartInteractions.AddCandle(candle);
    // This refreshes on each add!
}

// Good
foreach (var candle in candles)
{
    _boundCandles.Add(candle); // Direct add
}
_formsPlot.Refresh(); // Single refresh
```

### Preview Optimization

Previews are cleared and recreated on every mouse move. For complex shapes, consider:
- Throttling mouse move events
- Using simpler preview geometry
- Batch rendering

## Common Issues

### Issue: Event Handlers Not Firing

**Solution**: Ensure the form is shown before attaching:
```csharp
// Form_Load is the right place
private void Form_Load(object sender, EventArgs e)
{
    _chartInteractions.Attach(formsPlot);
}
```

### Issue: Memory Leaks

**Solution**: Always call Dispose:
```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        _chartInteractions?.Dispose();
    }
    base.Dispose(disposing);
}
```

### Issue: Pan/Zoom Not Working

**Solution**: Ensure draw mode is set to None:
```csharp
_chartInteractions.SetDrawMode(ChartDrawMode.None);
```

## Contributing

When contributing new features:

1. Follow existing code patterns
2. Add XML documentation comments
3. Implement proper disposal
4. Add TODO comments for partial implementations
5. Update this guide and README
6. Test on Windows with actual FormsPlot

## Resources

- [ScottPlot Documentation](https://scottplot.net/)
- [.NET Dependency Injection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [WinForms Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)
