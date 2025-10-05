using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

public class FibonacciRetracementStrategy : BaseDrawStrategy
{
    public FibonacciRetracementStrategy(IInteractionContext ctx) : base(ctx) { }

    public override void OnMouseDown(Coordinates coords)
    {
        Start = MaybeSnap(coords);
        var line = Ctx.FormsPlot.Plot.Add.Line(Start.Value, Start.Value);
        line.LineWidth = 1;
        line.LineColor = Colors.Gold.WithAlpha(0.5);
        Preview = line;
        Ctx.SetPreview(line);
        Ctx.Refresh();
    }

    public override void OnMouseMove(Coordinates coords)
    {
        if (Preview is ScottPlot.Plottables.Line line && Start is Coordinates s)
        {
            var end = MaybeSnap(coords);
            line.Coordinates = (s, end);
            Ctx.Refresh();
        }
    }

    public override void OnMouseUp(Coordinates coords)
    {
        if (Start is null) return;
        var end = MaybeSnap(coords);
        var line = Ctx.FormsPlot.Plot.Add.Line(Start.Value, end);
        line.LineWidth = 2;
        line.LineColor = Colors.Gold;
        Ctx.AddFinal(line);
        Ctx.SetPreview(null);
        Preview = null;
        Start = null;
        Ctx.Refresh();
    }
}
