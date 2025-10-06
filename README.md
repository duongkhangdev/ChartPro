# ChartPro

A professional WinForms trading chart application built with ScottPlot 5 and .NET 8, featuring dependency injection-based architecture.

## Features

- **DI-Based Architecture**: Leverages Microsoft.Extensions.DependencyInjection for clean, testable code
- **Chart Interactions Service**: `IChartInteractions` service manages all chart interactions, drawing tools, and real-time updates
- **Shape Management**: `IShapeManager` service with Command pattern for shape operations
  - Centralized shape tracking and management
  - Undo/Redo support via keyboard shortcuts (Ctrl+Z/Ctrl+Y)
  - Command pattern for extensible operations
- **Drawing Tools**: 
  - Trend lines
  - Horizontal and vertical lines
  - Rectangles
  - Circles
  - Fibonacci retracement (with all standard levels and price labels)
  - Fibonacci extension (with projection levels)
- **Real-Time Updates**: Support for live candle updates via `BindCandles()`, `UpdateLastCandle()`, and `AddCandle()`
- **Keyboard Shortcuts**: Ctrl+Z (undo), Ctrl+Y/Ctrl+Shift+Z (redo), Delete (delete selected), Esc (cancel drawing)
- **Memory-Safe**: Proper event handler cleanup to prevent memory leaks
- **ScottPlot 5 Integration**: Built on the latest ScottPlot 5 for high-performance charting
- **Unit Tests**: Comprehensive test coverage for Command pattern and ShapeManager

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
│   ├── Commands/                      # Command pattern for undo/redo
│   │   ├── ICommand.cs               # Command interface
│   │   ├── AddShapeCommand.cs        # Add shape command
│   │   └── DeleteShapeCommand.cs     # Delete shape command
│   ├── ShapeManagement/               # Shape management service
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

1. **IShapeManager / ShapeManager**: Shape management service with undo/redo
   - Tracks all shapes and their metadata
   - Command pattern implementation for operations
   - `AddShape()`: Adds a shape to the chart
   - `DeleteShape()`: Removes a shape from the chart
   - `Undo()` / `Redo()`: Undo/redo operations
   - Exposes `CanUndo`, `CanRedo` properties

2. **ICommand Pattern**: Command pattern for extensible operations
   - `ICommand`: Interface for commands
   - `AddShapeCommand`: Command to add a shape
   - `DeleteShapeCommand`: Command to delete a shape
   - Execute/Undo methods for reversible operations

3. **IChartInteractions / ChartInteractions**: Chart interaction service
   - `Attach()`: Attaches to a FormsPlot control
   - `SetDrawMode()`: Changes drawing mode
   - `BindCandles()`, `UpdateLastCandle()`, `AddCandle()`: Real-time data management
   - Integrates with ShapeManager for shape operations
   - Handles mouse events (MouseDown, MouseMove, MouseUp)
   - Uses strategy pattern to delegate drawing logic to mode-specific strategies
   - Manages shape previews during drawing
   - Implements `IDisposable` for proper cleanup

4. **Program.cs**: DI Container Setup
   - Registers `IShapeManager` service
   - Registers `IChartInteractions` service
   - Configures application startup

5. **MainForm**: UI with integrated services
   - Receives `IChartInteractions` via constructor injection
   - Provides toolbar for drawing mode selection
   - Keyboard shortcuts for undo/redo (Ctrl+Z, Ctrl+Y)
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

# Run tests (Windows only)
dotnet test ChartPro.Tests/ChartPro.Tests.csproj
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
6. Pan/zoom is disabled during drawing, enabled otherwise
7. **Undo/Redo**: Use Ctrl+Z to undo and Ctrl+Y to redo shape operations

## CI/CD

The project includes a GitHub Actions workflow (`.github/workflows/build-and-release.yml`) that:
- Builds the project on Windows runners
- Creates source code archives
- Attaches artifacts to releases (on tag push)

## Repository Configuration

The repository is configured with best practices for maintenance and collaboration:
- **Automatic Branch Deletion**: Enabled - head branches are automatically deleted after PR merge
- **Settings as Code**: Repository settings are defined in `.github/settings.yml`
- See [REPOSITORY_SETTINGS.md](REPOSITORY_SETTINGS.md) for complete configuration documentation

## TODO / Future Enhancements

The following features are planned for future implementation:
- Customizable Fibonacci levels (show/hide specific levels)
- Custom level ratios
- Channel drawing
- Triangle drawing tool
- Text annotation tool
- Shape selection for interactive editing (move, resize)
- Shape deletion via UI (currently only via undo)
- Persistence of drawn shapes
- Additional technical indicators

## License

This project is licensed under the terms specified in the repository.
