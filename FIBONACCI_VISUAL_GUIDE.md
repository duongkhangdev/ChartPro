# Fibonacci Tools Visual Guide

## UI Layout

### Toolbar Buttons
The toolbar on the right side of the application now includes two new buttons:

```
┌──────────────────────┐
│  None                │
├──────────────────────┤
│  Trend Line          │
├──────────────────────┤
│  Horizontal Line     │
├──────────────────────┤
│  Vertical Line       │
├──────────────────────┤
│  Rectangle           │
├──────────────────────┤
│  Circle              │
├──────────────────────┤
│  Fib Retracement  ◄──│ NEW
├──────────────────────┤
│  Fib Extension    ◄──│ NEW
├──────────────────────┤
│                      │
│  Generate Sample     │
│  Data                │
└──────────────────────┘
```

## Fibonacci Retracement Visualization

### Example: Uptrend (Price from $100 to $150)

```
Price
│
$150.00 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  1.0 (150.00)      [RED]
│                                          ▲
$139.30 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.786 (139.30)   [PURPLE]
│                                          │
$130.90 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.618 (130.90)   [BLUE] ★
│                                          │
$125.00 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.5 (125.00)     [GREEN]
│                                          │
$119.10 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.382 (119.10)   [YELLOW]
│                                          │
$111.80 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.236 (111.80)   [ORANGE]
│                                          │
$100.00 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.0 (100.00)     [RED]
│
└────────────────────────────────────────► Time

★ = Golden Ratio (most significant level)
```

### Drawing Process (Retracement)

**Step 1 - Click "Fib Retracement"**
```
Button becomes highlighted (blue background, white text)
Pan/zoom is disabled
```

**Step 2 - Click at starting point ($100)**
```
No visual change yet
Starting coordinates captured
```

**Step 3 - Drag to ending point ($150)**
```
Preview appears:
- All 7 levels shown as gray semi-transparent lines
- No labels (preview mode)
- Updates in real-time as mouse moves
```

**Step 4 - Release mouse**
```
Final drawing appears:
- 7 colored horizontal lines
- Each line has label on right side: "ratio (price)"
- Button returns to normal state
- Pan/zoom re-enabled
```

## Fibonacci Extension Visualization

### Example: Uptrend with Extensions (Price from $100 to $150)

```
Price
│
$230.90 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  2.618 (230.90)   [DARK BLUE]
│                                          ▲
$200.00 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  2.0 (200.00)     [DARK RED]
│                                          │
$180.90 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  1.618 (180.90)   [MAGENTA] ★
│                                          │
$163.60 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  1.272 (163.60)   [CYAN]
│                                          │
│                                          │ EXTENSION ZONE
├ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┤
│                                          │ ORIGINAL MOVE
$150.00 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  1.0 (150.00)     [RED]
$139.30 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.786 (139.30)   [PURPLE]
$130.90 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.618 (130.90)   [BLUE]
$125.00 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.5 (125.00)     [GREEN]
$119.10 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.382 (119.10)   [YELLOW]
$111.80 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.236 (111.80)   [ORANGE]
$100.00 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.0 (100.00)     [RED]
│
└────────────────────────────────────────► Time

★ = Golden Extension (most significant target)
```

## Downtrend Example

### Fibonacci Retracement (Price from $150 to $100)

```
Price
│
$150.00 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.0 (150.00)     [RED]
│                                          │
$138.20 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.236 (138.20)   [ORANGE]
│                                          │
$130.90 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.382 (130.90)   [YELLOW]
│                                          │
$125.00 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.5 (125.00)     [GREEN]
│                                          │
$119.10 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.618 (119.10)   [BLUE] ★
│                                          │
$110.70 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  0.786 (110.70)   [PURPLE]
│                                          ▼
$100.00 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  1.0 (100.00)     [RED]
│
└────────────────────────────────────────► Time

Drawing: Click at $150 (high), drag to $100 (low)
Interpretation: Levels show potential bounce/resistance points
```

## Label Format Details

Each level label consists of:
```
┌─────────────────────┐
│ 0.618 (130.90)      │
│  ↑        ↑         │
│  │        │         │
│  │        └─ Actual price at this level
│  └─ Fibonacci ratio
└─────────────────────┘
```

**Label Styling**:
- Font: Sans-serif, 10pt
- Background: White with 70% opacity
- Border: 1px solid, color matches level color
- Padding: 3px
- Position: Right edge of chart (at maxX)
- Alignment: Left-aligned

## Color Reference

### Retracement Levels
| Level | Color  | RGB/Hex    | Significance           |
|-------|--------|------------|------------------------|
| 0.0   | Red    | #FF0000    | Start point            |
| 0.236 | Orange | #FFA500    | Minor retracement      |
| 0.382 | Yellow | #FFFF00    | Shallow retracement    |
| 0.5   | Green  | #008000    | Midpoint               |
| 0.618 | Blue   | #0000FF    | Golden ratio ★         |
| 0.786 | Purple | #800080    | Deep retracement       |
| 1.0   | Red    | #FF0000    | End point              |

### Extension Levels (Additional)
| Level | Color      | RGB/Hex    | Significance           |
|-------|------------|------------|------------------------|
| 1.272 | Cyan       | #00FFFF    | Minor extension        |
| 1.618 | Magenta    | #FF00FF    | Golden extension ★     |
| 2.0   | Dark Red   | #8B0000    | Double extension       |
| 2.618 | Dark Blue  | #00008B    | Extreme extension      |

## Interactive Features

### Preview Mode (While Drawing)
```
Appearance:
- All lines: Gray (#808080)
- Transparency: 50% (Alpha 0.5)
- Line width: 1px
- Labels: None (hidden)
- Updates: Real-time as mouse moves
```

### Final Mode (After Release)
```
Appearance:
- Lines: Individual colors (per level)
- Transparency: 100% (Opaque)
- Line width: 2px
- Labels: Visible with level + price
- Position: Fixed until manually cleared
```

### Button States
```
┌──────────────────────┐
│  Fib Retracement     │  ← Normal (gray background)
└──────────────────────┘

┌──────────────────────┐
│  Fib Retracement     │  ← Active (blue background, white text)
└──────────────────────┘
```

## Usage Workflow

### Complete Drawing Sequence

```
1. Generate sample data (or load your data)
   ↓
2. Click "Fib Retracement" or "Fib Extension" button
   ↓
3. Identify swing low and swing high on chart
   ↓
4. Click at first point (start)
   ↓
5. Drag to second point (end)
   [Preview shows gray levels]
   ↓
6. Release mouse button
   [Final colored levels with labels appear]
   ↓
7. Mode automatically returns to "None"
   [Pan/zoom re-enabled]
```

## Screen Layout

```
┌─────────────────────────────────────────────┬────────────┐
│                                             │   None     │
│                                             ├────────────┤
│                                             │ Trend Line │
│                                             ├────────────┤
│         Chart Area                          │Horizontal  │
│                                             ├────────────┤
│     (Candlesticks + Fibonacci Levels)       │ Vertical   │
│                                             ├────────────┤
│                                             │ Rectangle  │
│                                             ├────────────┤
│                                             │  Circle    │
│                                             ├────────────┤
│                                             │    Fib     │
│                                             │Retracement │
│                                             ├────────────┤
│                                             │    Fib     │
│                                             │ Extension  │
│                                             ├────────────┤
│                                             │            │
│                                             │  Generate  │
│                                             │   Sample   │
│                                             │    Data    │
└─────────────────────────────────────────────┴────────────┘
       1000px wide                                200px wide
```

## Multi-Tool Example

You can draw multiple Fibonacci tools on the same chart:

```
Price
│
│  ┌─ Second Fib (Minor swing)
│  │
│  $145 ━━  0.618 ━━━━━━━━┐
│  $140 ━━  0.5   ━━━━━━━━│
│  $135 ━━  0.382 ━━━━━━━━┘
│  │
│  └─ Primary Fib (Major swing)
│
$150 ━━━━━━━━━━━━━━━━━━━━  1.0
$130 ━━━━━━━━━━━━━━━━━━━━  0.618 ★
$125 ━━━━━━━━━━━━━━━━━━━━  0.5
$120 ━━━━━━━━━━━━━━━━━━━━  0.382
$100 ━━━━━━━━━━━━━━━━━━━━  0.0
│
└────────────────────────► Time

Multiple Fibonacci tools can coexist
Draw as many as needed for your analysis
```

## Tips for Visual Clarity

1. **Zoom In**: For closely-spaced levels, zoom in on the chart
2. **Use Extensions Wisely**: Only use extension levels when analyzing potential breakouts
3. **Combine with Candlesticks**: Fibonacci levels are most effective when viewed with price action
4. **Color Recognition**: Learn to quickly identify the golden ratios (blue 0.618, magenta 1.618)
5. **Multiple Timeframes**: Draw Fibonacci on different timeframes for confluence zones

## Known Visual Behaviors

1. **Label Overlap**: When levels are very close, labels may overlap
   - This is expected behavior
   - Zoom in for better visibility

2. **Chart Scaling**: Fibonacci tools may cause chart to auto-scale to include all levels
   - This is intentional to show all extension levels
   - Manually zoom if needed

3. **Label Position**: Labels always appear at the right edge (maxX)
   - Ensures labels don't interfere with price action
   - Consistent positioning across all draws

4. **Color Consistency**: Each level always uses the same color
   - 0.618 is always blue (most important retracement)
   - 1.618 is always magenta (most important extension)
   - Easy to compare multiple Fibonacci drawings
