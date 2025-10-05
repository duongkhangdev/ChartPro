# Fibonacci Tools Usage Guide

## Overview

ChartPro now includes comprehensive Fibonacci analysis tools for both retracement and extension levels, complete with color-coded levels and price labels.

## Features

### Fibonacci Retracement
**Purpose**: Identify potential support/resistance levels during price corrections

**Levels Included**:
- 0.0 (Red) - Start point
- 0.236 (Orange) - Minor retracement
- 0.382 (Yellow) - Shallow retracement
- 0.5 (Green) - Mid-point
- 0.618 (Blue) - Golden ratio (most significant)
- 0.786 (Purple) - Deep retracement
- 1.0 (Red) - End point

### Fibonacci Extension
**Purpose**: Project potential profit targets beyond the original move

**Levels Included**:
- All retracement levels (0.0 to 1.0)
- 1.272 (Cyan) - Minor extension
- 1.618 (Magenta) - Golden extension (most significant)
- 2.0 (Dark Red) - Double extension
- 2.618 (Dark Blue) - Extreme extension

## How to Use

### Drawing Fibonacci Retracement

1. **Click** the "Fib Retracement" button in the toolbar
2. **Identify the trend**: Find a significant price move (swing low to swing high, or high to low)
3. **Draw the tool**:
   - Click on the starting point (swing low for uptrend, swing high for downtrend)
   - Drag to the ending point (swing high for uptrend, swing low for downtrend)
   - Release the mouse button
4. **Review levels**: The tool will display all Fibonacci levels with their corresponding prices

### Drawing Fibonacci Extension

1. **Click** the "Fib Extension" button in the toolbar
2. **Identify the trend**: Find a completed price move
3. **Draw the tool**: Same as retracement (from start to end of move)
4. **Review projection levels**: Extension levels above 1.0 show potential profit targets

## Direction Support

The tools automatically work in both directions:

**Uptrend (Bullish)**:
- Draw from swing low (bottom) to swing high (top)
- Retracement levels show where price might pull back to
- Extension levels show where price might rally to

**Downtrend (Bearish)**:
- Draw from swing high (top) to swing low (bottom)
- Retracement levels show where price might bounce to
- Extension levels show where price might fall to

## Visual Features

### Color Coding
Each level has a unique color for easy identification:
- **Red**: Start/End points (0.0, 1.0)
- **Orange**: 0.236 level
- **Yellow**: 0.382 level
- **Green**: 0.5 level (midpoint)
- **Blue**: 0.618 level (golden ratio - most important)
- **Purple**: 0.786 level
- **Cyan**: 1.272 extension
- **Magenta**: 1.618 extension (golden extension - most important)
- **Dark Red**: 2.0 extension
- **Dark Blue**: 2.618 extension

### Price Labels
Each level displays:
- The Fibonacci ratio (e.g., "0.618")
- The actual price value at that level (e.g., "125.45")
- Format: "0.618 (125.45)"

### Preview Mode
While drawing:
- Semi-transparent gray lines show all levels
- No labels (for cleaner preview)
- Updates in real-time as you drag

### Final Mode
After releasing the mouse:
- Solid colored lines
- Price labels on the right side
- Permanent until manually cleared

## Trading Applications

### Retracement Levels
- **Entry Points**: Look for buying opportunities at 0.382, 0.5, or 0.618 in uptrends
- **Stop Loss**: Place stops below the 1.0 level (original low)
- **Support/Resistance**: These levels often act as support in uptrends and resistance in downtrends

### Extension Levels
- **Profit Targets**: Take profits at 1.272, 1.618, or 2.0 extensions
- **Breakout Targets**: After a retracement, price often rallies to extension levels
- **Trend Continuation**: Extension levels help identify how far a trend might go

## Technical Details

### Implementation
- **File**: `ChartPro/Charting/FibonacciTool.cs`
- **Levels**: `ChartPro/Charting/FibonacciLevel.cs`
- **Integration**: `ChartPro/Charting/Interactions/ChartInteractions.cs`

### Customization
The levels are defined in `FibonacciLevel.cs`:
```csharp
// Get default retracement levels
var levels = FibonacciLevel.GetDefaultRetracementLevels();

// Get extension levels (includes retracement + extensions)
var levels = FibonacciLevel.GetDefaultExtensionLevels();
```

Future enhancements will include:
- UI to toggle individual levels on/off
- Custom level ratios
- Saved level preferences
- Level tooltips with descriptions

## Examples

### Example 1: Uptrend Retracement
```
Price moves from $100 to $150
Draw Fibonacci from $100 (low) to $150 (high)
Levels displayed:
- 0.0: $100.00
- 0.236: $111.80
- 0.382: $119.10
- 0.5: $125.00
- 0.618: $130.90
- 0.786: $139.30
- 1.0: $150.00
```

### Example 2: Downtrend Extension
```
Price falls from $150 to $100
Draw Fibonacci from $150 (high) to $100 (low)
Extension levels:
- 1.0: $100.00 (original low)
- 1.272: $86.40 (first target)
- 1.618: $69.10 (golden extension target)
- 2.0: $50.00 (double extension)
- 2.618: $30.90 (extreme target)
```

## Tips

1. **Use Multiple Timeframes**: Draw Fibonacci tools on different timeframes for confluence
2. **Combine with Other Tools**: Use with trend lines, support/resistance for confirmation
3. **Focus on Key Levels**: 0.382, 0.5, 0.618 (retracement) and 1.618 (extension) are most reliable
4. **Wait for Confirmation**: Don't trade solely on Fibonacci levels; wait for price action confirmation
5. **Draw from Significant Swings**: Use major swing highs/lows, not minor fluctuations

## Troubleshooting

**Issue**: Levels appear too close together
- **Solution**: Zoom in on the chart for better visibility

**Issue**: Labels overlap
- **Solution**: This is expected when levels are close; you can zoom in or clear and redraw

**Issue**: Wrong direction
- **Solution**: The tool adapts to direction automatically based on start/end points. Just ensure you're drawing from the correct swing point.

## Future Enhancements

Planned features:
- [ ] Toggle individual level visibility
- [ ] Add custom level ratios
- [ ] Save/load level preferences
- [ ] Level tooltips with descriptions
- [ ] Three-point Fibonacci tool
- [ ] Fibonacci time zones
- [ ] Fibonacci arcs and fans
