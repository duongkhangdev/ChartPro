using ScottPlot;
using ScottPlot.WinForms;

namespace ChartPro.Charting.Commands;

/// <summary>
/// Command for adding a shape to the chart.
/// </summary>
public class AddShapeCommand : ICommand
{
    private readonly FormsPlot _formsPlot;
    private readonly IPlottable _shape;

    public IPlottable Shape => _shape;

    public AddShapeCommand(FormsPlot formsPlot, IPlottable shape)
    {
        _formsPlot = formsPlot ?? throw new ArgumentNullException(nameof(formsPlot));
        _shape = shape ?? throw new ArgumentNullException(nameof(shape));
    }

    public void Execute()
    {
        _formsPlot.Plot.Add.Plottable(_shape);
        _formsPlot.Refresh();
    }

    public void Undo()
    {
        _formsPlot.Plot.Remove(_shape);
        _formsPlot.Refresh();
    }
}
