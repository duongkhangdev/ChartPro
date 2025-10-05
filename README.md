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
- **Real-Time Updates**: Support for live candle updates via `BindCandles()`, `UpdateLastCandle()`, and `AddCandle()`
- **Memory-Safe**: Proper event handler cleanup to prevent memory leaks
- **ScottPlot 5 Integration**: Built on the latest ScottPlot 5 for high-performance charting

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
└── Strategies/                        # Strategy tests
    ├── TrendLineStrategyTests.cs
    ├── HorizontalLineStrategyTests.cs
    ├── VerticalLineStrategyTests.cs
    ├── RectangleStrategyTests.cs
    ├── CircleStrategyTests.cs
    ├── FibonacciRetracementStrategyTests.cs
    └── DrawModeStrategyFactoryTests.cs
```

### Key Components

1. **IChartInteractions**: Interface defining chart interaction operations
   - `Attach()`: Attaches to a FormsPlot control
   - `SetDrawMode()`: Changes drawing mode
   - `BindCandles()`, `UpdateLastCandle()`, `AddCandle()`: Real-time data management
   - Implements `IDisposable` for proper cleanup

2. **ChartInteractions**: Service implementation
   - Handles mouse events (MouseDown, MouseMove, MouseUp)
   - Uses strategy pattern to delegate drawing logic to mode-specific strategies
   - Manages shape previews during drawing
   - Finalizes shapes on mouse release
   - Safely unhooks event handlers on disposal

3. **Strategy Pattern**: Extensible drawing architecture
   - `IDrawModeStrategy`: Interface for draw mode implementations
   - `DrawModeStrategyFactory`: Creates appropriate strategy for each mode
   - Each strategy encapsulates preview and final shape creation logic
   - Easy to add new draw modes without modifying existing code

4. **Program.cs**: DI Container Setup
   - Registers `IChartInteractions` service
   - Configures application startup

5. **MainForm**: UI with integrated service
   - Receives `IChartInteractions` via constructor injection
   - Provides toolbar for drawing mode selection
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
3. Select a drawing tool from the right toolbar
4. Click and drag on the chart to draw
5. The drawing mode automatically resets to "None" after completing a shape
6. Pan/zoom is disabled during drawing, enabled otherwise

## CI/CD

The project includes a GitHub Actions workflow (`.github/workflows/build-and-release.yml`) that:
- Builds the project on Windows runners
- Creates source code archives
- Attaches artifacts to releases (on tag push)

## TODO / Future Enhancements

The following features are planned for future implementation:
- Full Fibonacci retracement with levels (0.236, 0.382, 0.5, 0.618, 0.786)
- Fibonacci extension tool
- Channel drawing
- Triangle drawing tool
- Text annotation tool
- Shape editing and deletion
- Persistence of drawn shapes
- Additional technical indicators

## License

This project is licensed under the terms specified in the repository.
