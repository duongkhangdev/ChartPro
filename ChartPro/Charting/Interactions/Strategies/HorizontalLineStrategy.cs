using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

public class HorizontalLineStrategy : BaseDrawStrategy
{
    public HorizontalLineStrategy(IInteractionContext ctx) : base(ctx) { }

    public override void OnMouseDown(Coordinates coords)
    {
        Start = MaybeSnap(coords);
        var y = Start.Value.Y;
        var h = Ctx.FormsPlot.Plot.Add.HorizontalLine(y);
        h.LineWidth = 1;
        h.LineColor = Colors.Gray.WithAlpha(0.5);
        Preview = h;
        Ctx.SetPreview(h);
        Ctx.Refresh();
    }

    public override void OnMouseMove(Coordinates coords)
    {
        if (Preview is ScottPlot.Plottables.HorizontalLine h)
        {
            var end = MaybeSnap(coords);
            h.Y = end.Y;
            Ctx.Refresh();
        }
    }

    public override void OnMouseUp(Coordinates coords)
    {
        var end = MaybeSnap(coords);
        var h = Ctx.FormsPlot.Plot.Add.HorizontalLine(end.Y);
        h.LineWidth = 2;
        h.LineColor = Colors.Green;
        Ctx.AddFinal(h);
        Ctx.SetPreview(null);
        Preview = null;
        Start = null;
        Ctx.Refresh();
    }
}
