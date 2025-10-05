using ChartPro.Charting.Shapes;
using ScottPlot;

namespace ChartPro.Charting.Commands;

/// <summary>
/// Command for deleting a shape from the chart.
/// </summary>
public class DeleteShapeCommand : ICommand
{
    private readonly IShapeManager _shapeManager;
    private readonly DrawnShape _shape;
    private readonly Plot _plot;

    public string Description => $"Delete {_shape.DrawMode} shape";

    public DeleteShapeCommand(IShapeManager shapeManager, DrawnShape shape, Plot plot)
    {
        _shapeManager = shapeManager ?? throw new ArgumentNullException(nameof(shapeManager));
        _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        _plot = plot ?? throw new ArgumentNullException(nameof(plot));
    }

    public void Execute()
    {
        _shapeManager.RemoveShape(_shape);
        _plot.Remove(_shape.Plottable);
    }

    public void Undo()
    {
        _shapeManager.AddShape(_shape);
        _plot.Add.Plottable(_shape.Plottable);
    }
}
