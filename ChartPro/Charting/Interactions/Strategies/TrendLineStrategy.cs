using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

public class TrendLineStrategy : BaseDrawStrategy
{
    public TrendLineStrategy(IInteractionContext ctx) : base(ctx) { }

    public override void OnMouseDown(Coordinates coords)
    {
        Start = MaybeSnap(coords);
        if (Start is Coordinates s)
        {
            var line = Ctx.FormsPlot.Plot.Add.Line(s, s);
            line.LineWidth = 1;
            line.LineColor = Colors.Gray.WithAlpha(0.5);
            Preview = line;
            Ctx.SetPreview(line);
            Ctx.Refresh();
        }
    }

    public override void OnMouseMove(Coordinates coords)
    {
        if (Start is null || Preview is null) return;
        var end = MaybeSnap(coords);
        if (Preview is ScottPlot.Plottables.Line line)
        {
            line.Coordinates = (Start.Value, end);
            Ctx.Refresh();
        }
    }

    public override void OnMouseUp(Coordinates coords)
    {
        if (Start is null) return;
        var end = MaybeSnap(coords);
        var final = Ctx.FormsPlot.Plot.Add.Line(Start.Value, end);
        final.LineWidth = 2;
        final.Color = Colors.Blue;
        Ctx.AddFinal(final);
        Ctx.SetPreview(null);
        Preview = null;
        Start = null;
        Ctx.Refresh();
    }
}
