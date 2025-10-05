using ChartPro.Charting.Shapes;
using ScottPlot;

namespace ChartPro.Charting.Commands;

/// <summary>
/// Command for adding a shape to the chart.
/// </summary>
public class AddShapeCommand : ICommand
{
    private readonly IShapeManager _shapeManager;
    private readonly DrawnShape _shape;
    private readonly Plot _plot;

    public string Description => $"Add {_shape.DrawMode} shape";

    public AddShapeCommand(IShapeManager shapeManager, DrawnShape shape, Plot plot)
    {
        _shapeManager = shapeManager ?? throw new ArgumentNullException(nameof(shapeManager));
        _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        _plot = plot ?? throw new ArgumentNullException(nameof(plot));
    }

    public void Execute()
    {
        _shapeManager.AddShape(_shape);
        _plot.Add.Plottable(_shape.Plottable);
    }

    public void Undo()
    {
        _shapeManager.RemoveShape(_shape);
        _plot.Remove(_shape.Plottable);
    }
}
