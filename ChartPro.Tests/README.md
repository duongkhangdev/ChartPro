# ChartPro.Tests

Unit test project for the ChartPro application.

## Test Coverage

This project contains comprehensive unit tests for:

### ShapeManagerTests
- Shape manager initialization
- Adding and removing shapes
- Shape lookup by ID
- Clear operation
- Shape collection management

### CommandTests
- AddShapeCommand execution and undo
- DeleteShapeCommand execution and undo
- Command execution through ShapeManager
- Undo/redo stack management
- Multiple undo/redo operations
- Redo stack clearing on new command

### DrawnShapeTests
- Shape construction and initialization
- Property management (IsSelected, IsVisible)
- Unique ID generation
- Metadata tracking (DrawMode, CreatedAt)
- Null parameter validation

## Running Tests

Tests require Windows with .NET Desktop runtime installed:

```bash
dotnet test ChartPro.Tests/ChartPro.Tests.csproj
```

On non-Windows platforms, tests will compile but cannot run due to WinForms dependencies.

## Test Framework

- **xUnit**: Test framework
- **Target Framework**: .NET 8.0 Windows
- **Dependencies**: ChartPro main project, ScottPlot

## Notes

- All tests are isolated and do not depend on external state
- Tests use ScottPlot's Plot class for creating test plottables
- No mocking required due to simple, testable design
