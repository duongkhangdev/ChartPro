# ChartPro UI Overview

## Application Window

The ChartPro application features a clean, professional interface designed for trading chart analysis.

```
┌─────────────────────────────────────────────────────────────────────┐
│ ChartPro - Trading Chart with ScottPlot 5                     [_][□][X] │
├─────────────────────────────────────────┬───────────────────────────┤
│                                         │  ┌─────────────────────┐  │
│                                         │  │       None          │  │
│                                         │  └─────────────────────┘  │
│                                         │  ┌─────────────────────┐  │
│        Chart Area                       │  │    Trend Line       │  │
│     (FormsPlot Control)                 │  └─────────────────────┘  │
│                                         │  ┌─────────────────────┐  │
│   - Interactive candlestick chart       │  │  Horizontal Line    │  │
│   - Drawing tools                       │  └─────────────────────┘  │
│   - Pan/Zoom when not drawing          │  ┌─────────────────────┐  │
│   - Real-time updates                   │  │   Vertical Line     │  │
│   - Preview during drawing              │  └─────────────────────┘  │
│                                         │  ┌─────────────────────┐  │
│                                         │  │     Rectangle       │  │
│                                         │  └─────────────────────┘  │
│                                         │  ┌─────────────────────┐  │
│                                         │  │     Fibonacci       │  │
│                                         │  └─────────────────────┘  │
│                                         │                           │
│                                         │  ─────────────────────────│
│                                         │  ┌─────────────────────┐  │
│                                         │  │ Generate Sample     │  │
│                                         │  │      Data           │  │
│                                         │  └─────────────────────┘  │
│                                         │   Toolbar (200px wide)    │
└─────────────────────────────────────────┴───────────────────────────┘
          1000px wide                              200px wide
```

## Window Dimensions

- **Total Width**: 1200px
- **Total Height**: 700px
- **Chart Area**: 1000px × 700px (Dock.Fill)
- **Toolbar**: 200px × 700px (Dock.Right)

## Components

### 1. Chart Area (Left Side)

**Type**: ScottPlot.WinForms.FormsPlot  
**Docked**: Fill

**Features**:
- Displays candlestick charts
- Interactive pan and zoom (when DrawMode = None)
- Drawing previews (semi-transparent gray)
- Final shapes (colored, solid)
- Coordinate-based positioning

**Behaviors**:
- **Pan**: Drag with left mouse when DrawMode = None
- **Zoom**: Scroll wheel when DrawMode = None
- **Draw**: Click and drag when DrawMode ≠ None

### 2. Toolbar (Right Side)

**Type**: Panel  
**Width**: 200px  
**Border**: FixedSingle  
**Background**: SystemColors.Control

**Buttons** (from top to bottom):
1. **None** - Disables drawing, enables pan/zoom
2. **Trend Line** - Draw diagonal lines
3. **Horizontal Line** - Draw horizontal price levels
4. **Vertical Line** - Draw vertical time lines
5. **Rectangle** - Draw rectangular zones
6. **Fibonacci** - Draw Fibonacci retracement

**Special Button**:
- **Generate Sample Data** - Creates 100 random OHLC candles

**Button Styling**:
- **Normal**: SystemColors.Control background
- **Active**: SystemColors.Highlight background, white text
- **Size**: 180px × 30px
- **Spacing**: 35px vertical gap between buttons

## User Interactions

### Drawing Workflow

1. **Select Tool**: Click a drawing tool button (e.g., "Trend Line")
   - Button highlights to show active state
   - Pan/zoom automatically disabled
   - Cursor remains default arrow

2. **Start Drawing**: Click on chart (MouseDown)
   - Captures starting coordinates
   - No visual change yet

3. **Preview**: Drag mouse (MouseMove)
   - Shows semi-transparent gray preview
   - Updates in real-time as you move
   - Shape follows cursor

4. **Finalize**: Release mouse (MouseUp)
   - Preview is removed
   - Final colored shape is added to chart
   - DrawMode automatically resets to "None"
   - Pan/zoom automatically re-enabled

### Data Generation

**Click "Generate Sample Data"**:
- Clears existing chart
- Generates 100 random OHLC candles
- Displays as candlestick plot
- Green candles = close > open
- Red candles = close < open
- Auto-scales axes

### Pan and Zoom

**When DrawMode = None**:
- **Left-Click Drag**: Pan chart
- **Scroll Wheel**: Zoom in/out
- **Right-Click**: Context menu (ScottPlot default)
- **Double-Click**: Auto-fit axes

## Visual Styles

### Chart Elements

**Candlesticks**:
- **Bullish**: Green body, black outline
- **Bearish**: Red body, black outline
- **Wick**: Thin line showing high/low

**Drawing Tools**:
- **Trend Line**: Blue, 2px width
- **Horizontal Line**: Green, 2px width
- **Vertical Line**: Orange, 2px width
- **Rectangle**: Purple outline, light purple fill (10% alpha)
- **Fibonacci**: Gold, 2px width

**Previews** (all tools):
- Gray color (#808080)
- 50% transparency
- 1px line width
- Thin/light appearance

### Chart Background

- **Plot Area**: White background
- **Axes**: Black lines
- **Grid**: Light gray dashed lines (ScottPlot default)
- **Labels**: Black text, sans-serif font

## Keyboard Shortcuts

Currently not implemented, but could be added:
- `Esc` - Cancel drawing, return to None mode
- `Delete` - Remove selected shape
- `Ctrl+Z` - Undo last shape
- `1-6` - Quick select drawing tools

## Real-Time Updates

The chart supports real-time data updates:

```csharp
// Initial binding
_chartInteractions.BindCandles(candleList);

// Update last candle (e.g., every second during trading)
_chartInteractions.UpdateLastCandle(updatedCandle);

// Add new candle (e.g., when time period changes)
_chartInteractions.AddCandle(newCandle);
```

**Behavior**:
- Updates are immediate (calls Refresh())
- Chart maintains zoom/pan position
- No flickering due to efficient ScottPlot rendering

## Accessibility

**Current State**:
- Standard Windows Forms controls (accessible by default)
- Keyboard navigation between toolbar buttons (Tab key)
- ToolTips could be added for better UX

**Improvements Possible**:
- Add ToolTips to buttons
- Add keyboard shortcuts
- Add status bar showing current mode
- Add coordinate display on mouse hover

## Performance

**Rendering**:
- Hardware-accelerated OpenGL (via ScottPlot)
- Smooth drawing preview updates
- No lag with 100+ candles
- Efficient memory usage

**Optimization**:
- Preview cleared before creating new one
- Event handlers properly managed
- No memory leaks (IDisposable pattern)

## Future Enhancements

Planned UI improvements:
1. Status bar showing:
   - Current draw mode
   - Mouse coordinates
   - Number of shapes drawn
2. Context menu for shape management:
   - Delete shape
   - Edit properties
   - Change color
3. Properties panel:
   - Line width selector
   - Color picker
   - Line style (solid, dashed, dotted)
4. Shape list panel:
   - List all drawn shapes
   - Toggle visibility
   - Quick select/edit
5. Fibonacci level customization:
   - Show/hide individual levels
   - Add custom levels
   - Show extensions

## Testing the UI

**Manual Testing Checklist**:
- [ ] Click each drawing tool button
- [ ] Draw each type of shape
- [ ] Verify preview appears during drawing
- [ ] Verify final shape appears on release
- [ ] Verify mode resets to None after drawing
- [ ] Verify pan/zoom works when mode is None
- [ ] Click "Generate Sample Data"
- [ ] Verify chart displays candlesticks
- [ ] Try multiple drawings in sequence
- [ ] Close application cleanly

**Visual Testing**:
- [ ] Buttons highlight correctly when active
- [ ] Previews are semi-transparent
- [ ] Final shapes are solid and colored
- [ ] Chart scales properly
- [ ] No overlapping UI elements
- [ ] Toolbar stays fixed at 200px width
