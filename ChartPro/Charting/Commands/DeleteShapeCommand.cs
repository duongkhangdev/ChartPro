using ScottPlot;
using ScottPlot.WinForms;

namespace ChartPro.Charting.Commands;

/// <summary>
/// Command for deleting a shape from the chart.
/// </summary>
public class DeleteShapeCommand : ICommand
{
    private readonly FormsPlot _formsPlot;
    private readonly IPlottable _shape;

    public IPlottable Shape => _shape;

    public DeleteShapeCommand(FormsPlot formsPlot, IPlottable shape)
    {
        _formsPlot = formsPlot ?? throw new ArgumentNullException(nameof(formsPlot));
        _shape = shape ?? throw new ArgumentNullException(nameof(shape));
    }

    public void Execute()
    {
        _formsPlot.Plot.Remove(_shape);
        _formsPlot.Refresh();
    }

    public void Undo()
    {
        _formsPlot.Plot.Add.Plottable(_shape);
        _formsPlot.Refresh();
    }
}
