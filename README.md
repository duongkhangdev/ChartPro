# ChartPro

A professional WinForms trading chart application built with ScottPlot 5 and .NET 8, featuring dependency injection-based architecture.

## Features

- **DI-Based Architecture**: Leverages Microsoft.Extensions.DependencyInjection for clean, testable code
- **Chart Interactions Service**: `IChartInteractions` service manages all chart interactions, drawing tools, and real-time updates
- **Drawing Tools**: 
  - Trend lines
  - Horizontal and vertical lines
  - Rectangles
  - Circles
  - Fibonacci retracement (with extensibility for additional tools)
- **Shape Management with Undo/Redo**: 
  - Command pattern implementation for all shape operations
  - Unlimited undo/redo stack via Ctrl+Z and Ctrl+Y
  - Shape selection with click (Ctrl+Click for multi-select)
  - Delete selected shapes with Delete key
  - Each shape tracked with metadata (ID, visibility, selection state, creation time)
- **Real-Time Updates**: Support for live candle updates via `BindCandles()`, `UpdateLastCandle()`, and `AddCandle()`
- **Keyboard Shortcuts**: Ctrl+Z (undo), Ctrl+Y/Ctrl+Shift+Z (redo), Delete (delete selected), Esc (cancel drawing)
- **Memory-Safe**: Proper event handler cleanup to prevent memory leaks
- **ScottPlot 5 Integration**: Built on the latest ScottPlot 5 for high-performance charting
- **Unit Tests**: Comprehensive test coverage for ShapeManager, Commands, and DrawnShape classes

## Architecture

ChartPro uses **Strategy and Factory patterns** for extensible drawing modes. Each draw mode is implemented as a separate strategy class, making it easy to add new tools without modifying existing code.

For detailed architecture documentation, see:
- [STRATEGY_PATTERN.md](STRATEGY_PATTERN.md) - Strategy/Factory pattern architecture
- [IMPLEMENTATION.md](IMPLEMENTATION.md) - Implementation details
- [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md) - Developer guidelines

### Project Structure

```
ChartPro/
├── Charting/
│   ├── ChartDrawMode.cs              # Drawing mode enumeration
│   ├── Commands/
│   │   ├── ICommand.cs               # Command pattern interface
│   │   ├── AddShapeCommand.cs        # Command to add a shape
│   │   └── DeleteShapeCommand.cs     # Command to delete a shape
│   ├── Shapes/
│   │   ├── DrawnShape.cs             # Shape wrapper with metadata
│   │   ├── IShapeManager.cs          # Shape manager interface
│   │   └── ShapeManager.cs           # Shape manager implementation
│   └── Interactions/
│       ├── IChartInteractions.cs     # Chart interactions interface
│       ├── ChartInteractions.cs      # DI-based service implementation
│       └── Strategies/               # Strategy pattern for draw modes
│           ├── IDrawModeStrategy.cs          # Strategy interface
│           ├── DrawModeStrategyFactory.cs    # Factory for creating strategies
│           ├── TrendLineStrategy.cs          # Trend line implementation
│           ├── HorizontalLineStrategy.cs     # Horizontal line implementation
│           ├── VerticalLineStrategy.cs       # Vertical line implementation
│           ├── RectangleStrategy.cs          # Rectangle implementation
│           ├── CircleStrategy.cs             # Circle/ellipse implementation
│           └── FibonacciRetracementStrategy.cs # Fibonacci implementation
├── MainForm.cs                        # Main application form with FormsPlot control
└── Program.cs                         # Application entry point with DI setup

ChartPro.Tests/                        # Unit test project
├── ShapeManagerTests.cs               # Tests for ShapeManager
├── CommandTests.cs                    # Tests for Command pattern
└── DrawnShapeTests.cs                 # Tests for DrawnShape
```

### Key Components

1. **IChartInteractions**: Interface defining chart interaction operations
   - `Attach()`: Attaches to a FormsPlot control
   - `SetDrawMode()`: Changes drawing mode
   - `BindCandles()`, `UpdateLastCandle()`, `AddCandle()`: Real-time data management
   - `Undo()`, `Redo()`: Undo/redo operations
   - `DeleteSelectedShapes()`: Deletes currently selected shapes
   - Implements `IDisposable` for proper cleanup

2. **ChartInteractions**: Service implementation
   - Handles mouse events (MouseDown, MouseMove, MouseUp)
   - Uses strategy pattern to delegate drawing logic to mode-specific strategies
   - Manages shape previews during drawing
   - Finalizes shapes on mouse release via Command pattern
   - Shape selection support (click to select, Ctrl+Click for multi-select)
   - Safely unhooks event handlers on disposal

3. **ShapeManager**: Centralized shape and command management
   - Tracks all drawn shapes with metadata
   - Maintains undo/redo command stacks
   - Provides shape lookup by ID
   - Supports shape addition/removal

4. **Command Pattern**: Implements undo/redo for all operations
   - `AddShapeCommand`: Adds a shape (can be undone)
   - `DeleteShapeCommand`: Deletes a shape (can be undone)
   - Commands are executed through ShapeManager

5. **Program.cs**: DI Container Setup
   - Registers `IChartInteractions` service
   - Configures application startup

6. **MainForm**: UI with integrated service
   - Receives `IChartInteractions` via constructor injection
   - Provides toolbar for drawing mode selection
   - Keyboard shortcuts for undo/redo/delete operations
   - Demonstrates sample data generation

6. **Unit Tests**: Comprehensive test coverage
   - Individual strategy tests validate each draw mode
   - Factory tests ensure correct strategy instantiation
   - Tests run on Windows with WinForms support

## Building

### Requirements

- .NET 8.0 SDK or later
- Windows OS (for WinForms)

### Build Commands

```bash
# Restore dependencies
dotnet restore ChartPro/ChartPro.csproj

# Build
dotnet build ChartPro/ChartPro.csproj --configuration Release

# Run
dotnet run --project ChartPro/ChartPro.csproj
```

## Usage

1. Launch the application
2. Click "Generate Sample Data" to load candlestick data
3. Select a drawing tool from the right toolbar or use keyboard shortcuts:
   - Press `1` for Trend Line
   - Press `2` for Horizontal Line
   - Press `3` for Vertical Line
   - Press `4` for Rectangle
   - Press `5` for Circle
   - Press `6` for Fibonacci Retracement
   - Press `ESC` to cancel drawing and return to pan/zoom mode
4. Click and drag on the chart to draw
5. The drawing mode automatically resets to "None" after completing a shape
6. Monitor the status bar at the bottom for:
   - Current drawing mode
   - Mouse coordinates (X, Y)
   - Shape parameters (length, angle, size) during drawing
7. Pan/zoom is disabled during drawing, enabled otherwise

### Shape Management

- **Select shapes**: Click on a shape when in "None" mode to select it
- **Multi-select**: Hold Ctrl and click to select multiple shapes
- **Delete**: Press Delete key to remove selected shapes
- **Undo**: Press Ctrl+Z to undo the last operation
- **Redo**: Press Ctrl+Y or Ctrl+Shift+Z to redo
- **Cancel drawing**: Press Esc to cancel the current drawing mode

## CI/CD

The project includes a GitHub Actions workflow (`.github/workflows/build-and-release.yml`) that:
- Builds the project on Windows runners
- Creates source code archives
- Attaches artifacts to releases (on tag push)

## Testing

The project includes a comprehensive unit test suite in the `ChartPro.Tests` project:

```bash
# Run tests (requires Windows with .NET Desktop runtime)
dotnet test ChartPro.Tests/ChartPro.Tests.csproj
```

Tests cover:
- ShapeManager operations (add, remove, clear, lookup)
- Command pattern (execute, undo, redo)
- DrawnShape metadata and state
- Undo/redo stack behavior

## TODO / Future Enhancements

The following features are planned for future implementation:
- Full Fibonacci retracement with levels (0.236, 0.382, 0.5, 0.618, 0.786)
- Fibonacci extension tool
- Channel drawing
- Triangle drawing tool
- Text annotation tool
- Shape move and resize operations
- Improved shape selection with visual feedback (highlighted outlines)
- Persistence of drawn shapes (save/load to file)
- Additional technical indicators

## License

This project is licensed under the terms specified in the repository.
