using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

public class VerticalLineStrategy : BaseDrawStrategy
{
    public VerticalLineStrategy(IInteractionContext ctx) : base(ctx) { }

    public override void OnMouseDown(Coordinates coords)
    {
        Start = MaybeSnap(coords);
        var x = Start.Value.X;
        var v = Ctx.FormsPlot.Plot.Add.VerticalLine(x);
        v.LineWidth = 1;
        v.LineColor = Colors.Gray.WithAlpha(0.5);
        Preview = v;
        Ctx.SetPreview(v);
        Ctx.Refresh();
    }

    public override void OnMouseMove(Coordinates coords)
    {
        if (Preview is ScottPlot.Plottables.VerticalLine v)
        {
            var end = MaybeSnap(coords);
            v.X = end.X;
            Ctx.Refresh();
        }
    }

    public override void OnMouseUp(Coordinates coords)
    {
        var end = MaybeSnap(coords);
        var v = Ctx.FormsPlot.Plot.Add.VerticalLine(end.X);
        v.LineWidth = 2;
        v.LineColor = Colors.Orange;
        Ctx.AddFinal(v);
        Ctx.SetPreview(null);
        Preview = null;
        Start = null;
        Ctx.Refresh();
    }
}
