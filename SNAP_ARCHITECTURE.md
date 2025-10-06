# Snap/Magnet Feature Architecture

## Component Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                           MainForm                              │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Toolbar Controls                                         │ │
│  │  • CheckBox: Enable Snap                                  │ │
│  │  • RadioButton: No Snap                                   │ │
│  │  • RadioButton: Snap to Price Grid                        │ │
│  │  • RadioButton: Snap to Candle OHLC                       │ │
│  └───────────────────────────────────────────────────────────┘ │
│                          ▼ Events                               │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  IChartInteractions (Interface)                           │ │
│  │  • bool SnapEnabled { get; set; }                         │ │
│  │  • SnapMode SnapMode { get; set; }                        │ │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    ChartInteractions (Service)                   │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  State Management                                         │ │
│  │  • _snapEnabled: bool                                     │ │
│  │  • _snapMode: SnapMode                                    │ │
│  │  • _shiftKeyPressed: bool                                 │ │
│  │  • _boundCandles: List<OHLC>                              │ │
│  └───────────────────────────────────────────────────────────┘ │
│                             ▼                                    │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Event Handlers                                           │ │
│  │  • OnKeyDown(KeyEventArgs) → Track Shift                 │ │
│  │  • OnKeyUp(KeyEventArgs) → Track Shift                   │ │
│  │  • OnMouseDown(MouseEventArgs) → Apply Snap              │ │
│  │  • OnMouseMove(MouseEventArgs) → Apply Snap              │ │
│  │  • OnMouseUp(MouseEventArgs) → Apply Snap                │ │
│  └───────────────────────────────────────────────────────────┘ │
│                             ▼                                    │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Snap Logic (Core)                                        │ │
│  │  ┌─────────────────────────────────────────────────────┐ │ │
│  │  │ ApplySnap(Coordinates)                              │ │ │
│  │  │  • Check if snap enabled OR Shift pressed           │ │ │
│  │  │  • Route to appropriate snap method                 │ │ │
│  │  └─────────────────────────────────────────────────────┘ │ │
│  │              ▼                        ▼                    │ │
│  │  ┌──────────────────────┐  ┌──────────────────────────┐ │ │
│  │  │ SnapToPrice()        │  │ SnapToCandleOHLC()       │ │ │
│  │  │  • Calculate grid    │  │  • Find nearest candle   │ │ │
│  │  │  • Round Y to grid   │  │  • Get OHLC values       │ │ │
│  │  │  • Snap X to candle  │  │  • Find closest price    │ │ │
│  │  └──────────────────────┘  └──────────────────────────┘ │ │
│  │              ▼                        ▼                    │ │
│  │  ┌──────────────────────┐  ┌──────────────────────────┐ │ │
│  │  │ Helper Methods       │  │ Helper Methods           │ │ │
│  │  │  • Calculate         │  │  • FindNearestCandle()   │ │ │
│  │  │    PriceGridSize()   │  │  • SnapToNearestCandle   │ │ │
│  │  │                      │  │    Time()                │ │ │
│  │  └──────────────────────┘  └──────────────────────────┘ │ │
│  └───────────────────────────────────────────────────────────┘ │
│                             ▼                                    │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Drawing Methods (Unchanged)                              │ │
│  │  • CreateTrendLine(), CreateRectangle(), etc.             │ │
│  │  • Receive snapped coordinates                            │ │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                        ScottPlot Chart                           │
│  Rendered shapes with snapped coordinates                        │
└─────────────────────────────────────────────────────────────────┘
```

## Data Flow: Drawing with Snap

### Scenario 1: Checkbox Enabled + Price Grid Mode

```
User clicks Enable Snap checkbox
    ▼
MainForm sets _chartInteractions.SnapEnabled = true
    ▼
User selects "Snap to Price Grid"
    ▼
MainForm sets _chartInteractions.SnapMode = SnapMode.Price
    ▼
User selects Trend Line tool
    ▼
User clicks mouse (MouseDown)
    ▼
OnMouseDown receives pixel coordinates
    ▼
Convert to chart coordinates
    ▼
ApplySnap(coords)
    ├─ Check: _snapEnabled = true ✓
    ├─ Check: _snapMode = SnapMode.Price ✓
    └─ Call: SnapToPrice(coords)
        ├─ Calculate visible price range
        ├─ Calculate appropriate grid size
        ├─ Round Y to grid: Y = Round(Y / gridSize) * gridSize
        ├─ Snap X to nearest candle time
        └─ Return snapped Coordinates
    ▼
Store snapped coordinates as _drawStartCoordinates
    ▼
User moves mouse (MouseMove)
    ▼
OnMouseMove receives pixel coordinates
    ▼
Convert to chart coordinates
    ▼
ApplySnap(coords) → snapped end coordinates
    ▼
UpdatePreview(_drawStartCoordinates, snappedEndCoords)
    ▼
User releases mouse (MouseUp)
    ▼
OnMouseUp receives pixel coordinates
    ▼
Convert to chart coordinates
    ▼
ApplySnap(coords) → final snapped end coordinates
    ▼
FinalizeShape(_drawStartCoordinates, snappedEndCoords)
    ▼
Create permanent line plottable with snapped coordinates
    ▼
Add to ScottPlot and refresh
```

### Scenario 2: Shift Key Override (Checkbox Disabled)

```
User does NOT check Enable Snap
    ▼
_chartInteractions.SnapEnabled = false
    ▼
User selects "Snap to Candle OHLC"
    ▼
_chartInteractions.SnapMode = SnapMode.CandleOHLC
    ▼
User selects Horizontal Line tool
    ▼
User holds Shift key
    ▼
OnKeyDown detects Shift → _shiftKeyPressed = true
    ▼
User clicks mouse (MouseDown)
    ▼
OnMouseDown receives coordinates
    ▼
ApplySnap(coords)
    ├─ Check: _snapEnabled = false
    ├─ Check: _shiftKeyPressed = true ✓
    ├─ shouldSnap = false || true = true ✓
    ├─ Check: _snapMode = SnapMode.CandleOHLC ✓
    └─ Call: SnapToCandleOHLC(coords)
        ├─ FindNearestCandle(coords.X)
        ├─ Get OHLC values [Open, High, Low, Close]
        ├─ Find closest to coords.Y
        ├─ Snap X to candle DateTime
        └─ Return snapped Coordinates
    ▼
[Drawing continues as in Scenario 1]
    ▼
User releases Shift key
    ▼
OnKeyUp detects Shift → _shiftKeyPressed = false
    ▼
Snap disabled for subsequent operations
```

## Key Design Decisions

### 1. Non-Invasive Integration
- Snap logic isolated in dedicated methods
- Existing drawing methods unchanged
- Coordinates snapped BEFORE entering drawing logic

### 2. Dual Enable Mechanism
```csharp
bool shouldSnap = _snapEnabled || _shiftKeyPressed;
```
- Checkbox: persistent across operations
- Shift key: temporary override
- OR logic allows either to activate

### 3. Dynamic Grid Calculation
```csharp
// Adapts to visible range
double magnitude = Math.Pow(10, Math.Floor(Math.Log10(range)));
```
- Grid spacing scales with zoom level
- Always displays appropriate granularity
- No fixed grid definitions needed

### 4. Efficient Candle Lookup
```csharp
return _boundCandles
    .OrderBy(candle => Math.Abs((candle.DateTime - targetTime).TotalSeconds))
    .FirstOrDefault();
```
- Single LINQ query per snap operation
- Acceptable performance for typical datasets
- Could be optimized with binary search if needed

### 5. Enum-Based Mode Selection
```csharp
public enum SnapMode { None, Price, CandleOHLC }
```
- Type-safe mode selection
- Easy to extend with new modes
- Clear intent in code

## Extension Points

### Adding New Snap Modes

1. Add to SnapMode enum:
```csharp
public enum SnapMode
{
    None,
    Price,
    CandleOHLC,
    TechnicalIndicator  // New mode
}
```

2. Add snap method:
```csharp
private Coordinates SnapToTechnicalIndicator(Coordinates coords)
{
    // Implementation
}
```

3. Update ApplySnap switch:
```csharp
return _snapMode switch
{
    SnapMode.Price => SnapToPrice(coords),
    SnapMode.CandleOHLC => SnapToCandleOHLC(coords),
    SnapMode.TechnicalIndicator => SnapToTechnicalIndicator(coords),
    _ => coords
};
```

4. Add UI control in MainForm

### Performance Optimization

For large datasets (>10,000 candles):
```csharp
// Build indexed time lookup
private Dictionary<DateTime, OHLC> _candleIndex;

private OHLC? FindNearestCandleOptimized(double x)
{
    var targetTime = DateTime.FromOADate(x);
    // Binary search on sorted times
    // O(log n) instead of O(n log n)
}
```

## Testing Strategy

See `SNAP_FEATURE_TESTING.md` for detailed test scenarios covering:
- Basic enable/disable
- Each snap mode
- Shift key override
- All drawing tools
- Zoom level independence
- Mode switching

## Conclusion

The snap feature architecture is:
- ✅ Modular and extensible
- ✅ Non-invasive to existing code
- ✅ User-friendly (dual enable methods)
- ✅ Performance-conscious
- ✅ Type-safe and maintainable
