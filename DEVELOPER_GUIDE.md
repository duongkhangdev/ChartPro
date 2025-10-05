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
// - FibonacciRetracement (fully implemented with all levels)
// - FibonacciExtension (fully implemented with projection levels)
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

### Adding New Drawing Tools

1. **Add to ChartDrawMode enum** (ChartDrawMode.cs)
```csharp
public enum ChartDrawMode
{
    // ... existing modes
    MyNewTool
}
```

2. **Implement in ChartInteractions** (ChartInteractions.cs)

Add preview method:
```csharp
private IPlottable CreateMyNewToolPreview(Coordinates start, Coordinates end)
{
    // Create preview with gray, semi-transparent style
    var tool = _formsPlot!.Plot.Add.MyNewTool(start, end);
    tool.LineWidth = 1;
    tool.LineColor = Colors.Gray.WithAlpha(0.5);
    return tool;
}
```

Add final method:
```csharp
private IPlottable CreateMyNewTool(Coordinates start, Coordinates end)
{
    // Create final with colored, solid style
    var tool = _formsPlot!.Plot.Add.MyNewTool(start, end);
    tool.LineWidth = 2;
    tool.LineColor = Colors.Blue;
    return tool;
}
```

Update UpdatePreview method:
```csharp
_previewPlottable = _currentDrawMode switch
{
    // ... existing cases
    ChartDrawMode.MyNewTool => CreateMyNewToolPreview(start, end),
    _ => null
};
```

Update FinalizeShape method:
```csharp
IPlottable? plottable = _currentDrawMode switch
{
    // ... existing cases
    ChartDrawMode.MyNewTool => CreateMyNewTool(start, end),
    _ => null
};
```

3. **Add UI Button** (MainForm.cs)
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

### Fibonacci Tools Implementation

The Fibonacci tools use a custom plottable implementation:

**Key Components**:
1. **FibonacciLevel.cs**: Defines level properties (ratio, label, color, visibility)
2. **FibonacciTool.cs**: Custom IPlottable that renders multiple levels with labels

**Example - Using Fibonacci Levels**:
```csharp
// Get default retracement levels
var levels = FibonacciLevel.GetDefaultRetracementLevels();
// Returns: 0.0, 0.236, 0.382, 0.5, 0.618, 0.786, 1.0

// Get extension levels (includes retracement + extensions)
var extLevels = FibonacciLevel.GetDefaultExtensionLevels();
// Returns: all retracement levels + 1.272, 1.618, 2.0, 2.618

// Create custom levels
var customLevels = new List<FibonacciLevel>
{
    new FibonacciLevel(0.382, "0.382", Colors.Yellow, isVisible: true),
    new FibonacciLevel(0.618, "0.618", Colors.Blue, isVisible: true),
    new FibonacciLevel(1.618, "1.618", Colors.Magenta, isVisible: true)
};

// Create Fibonacci tool
var fibTool = new FibonacciTool(startCoord, endCoord, customLevels, isPreview: false);
```

**Features**:
- Automatic price calculation based on coordinate range
- Color-coded levels for easy identification
- Inline labels showing ratio and actual price
- Preview mode (semi-transparent, no labels)
- Final mode (solid colors with labels)
- Direction-agnostic (works for uptrends and downtrends)

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
