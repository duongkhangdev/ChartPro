# Snap/Magnet Feature Testing Guide

## Overview
This document provides test scenarios for the snap/magnet feature implementation.

## Prerequisites
1. Build and run the ChartPro application
2. Click "Generate Sample Data" to load candlestick data

## Test Scenarios

### Test 1: Basic Snap Enable/Disable
**Objective**: Verify snap can be enabled and disabled via UI checkbox

**Steps**:
1. Ensure "Enable Snap" checkbox is unchecked
2. Select "Trend Line" tool
3. Draw a line - should place exactly where mouse clicks
4. Check "Enable Snap" checkbox
5. Select "Snap to Price Grid" radio button
6. Draw another trend line - coordinates should snap to grid

**Expected Result**: 
- With snap disabled: precise placement
- With snap enabled: coordinates align to grid

### Test 2: Snap to Price Grid
**Objective**: Verify price grid snapping works correctly

**Steps**:
1. Check "Enable Snap" checkbox
2. Select "Snap to Price Grid" radio button
3. Test with different drawing tools:
   - Trend Line
   - Horizontal Line
   - Vertical Line
   - Rectangle
   - Circle

**Expected Result**: 
- Y coordinates snap to rounded price levels
- X coordinates snap to nearest candle time
- Grid spacing adjusts based on visible price range

### Test 3: Snap to Candle OHLC
**Objective**: Verify snapping to candle OHLC values

**Steps**:
1. Check "Enable Snap" checkbox
2. Select "Snap to Candle OHLC" radio button
3. Draw a horizontal line near a candle
4. Observe Y coordinate snaps to nearest Open/High/Low/Close

**Expected Result**:
- Y coordinate snaps to one of the four OHLC values
- X coordinate snaps to candle time position

### Test 4: Shift Key Temporary Snap
**Objective**: Verify Shift key enables snap temporarily

**Steps**:
1. Uncheck "Enable Snap" checkbox
2. Select "Snap to Price Grid" radio button
3. Select "Trend Line" tool
4. Draw a line WITHOUT holding Shift - no snap
5. Hold Shift and draw another line - should snap

**Expected Result**:
- Without Shift: no snapping
- With Shift held: snapping active

### Test 5: Snap Mode Switching
**Objective**: Verify switching between snap modes

**Steps**:
1. Check "Enable Snap" checkbox
2. Select "Snap to Price Grid"
3. Draw a horizontal line - observe behavior
4. Switch to "Snap to Candle OHLC"
5. Draw another horizontal line - observe different behavior
6. Switch to "No Snap"
7. Draw a third line - no snapping

**Expected Result**: Each mode produces different snapping behavior

### Test 6: All Drawing Tools Compatibility
**Objective**: Verify snap works with all drawing tools

**Steps**:
Test snap feature with each tool:
1. Trend Line
2. Horizontal Line
3. Vertical Line
4. Rectangle
5. Circle
6. Fibonacci

**Expected Result**: Snap works consistently across all tools

### Test 7: Zoom Level Independence
**Objective**: Verify snap works at different zoom levels

**Steps**:
1. Enable "Snap to Price Grid"
2. Draw a line at default zoom
3. Zoom in significantly
4. Draw another line - grid spacing should adapt
5. Zoom out significantly
6. Draw another line - grid spacing should adapt

**Expected Result**: Grid spacing adjusts appropriately for visible range

## Known Limitations
- Snap is applied to both start and end points of shapes
- No visual indicator showing snap points (could be future enhancement)
- Price grid is calculated dynamically, not from a fixed set

## Performance Considerations
- Snap calculations are lightweight
- No noticeable impact on drawing responsiveness
- Candle lookup uses LINQ OrderBy (acceptable for typical datasets)

## Future Enhancements
- Visual snap indicators (e.g., highlight snap points)
- Snap to technical indicator levels
- Configurable snap tolerance
- Snap to previously drawn shapes
