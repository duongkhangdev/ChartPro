using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

public class RectangleStrategy : BaseDrawStrategy
{
    public RectangleStrategy(IInteractionContext ctx) : base(ctx) { }

    public override void OnMouseDown(Coordinates coords)
    {
        Start = MaybeSnap(coords);
        var rect = Ctx.FormsPlot.Plot.Add.Rectangle(Start.Value.X, Start.Value.X, Start.Value.Y, Start.Value.Y);
        rect.LineWidth = 1;
        rect.LineColor = Colors.Gray.WithAlpha(0.5);
        rect.FillColor = Colors.Gray.WithAlpha(0.1);
        Preview = rect;
        Ctx.SetPreview(rect);
        Ctx.Refresh();
    }

    public override void OnMouseMove(Coordinates coords)
    {
        if (Preview is ScottPlot.Plottables.Rectangle rect && Start is Coordinates s)
        {
            var end = MaybeSnap(coords);
            rect.XMin = Math.Min(s.X, end.X);
            rect.XMax = Math.Max(s.X, end.X);
            rect.YMin = Math.Min(s.Y, end.Y);
            rect.YMax = Math.Max(s.Y, end.Y);
            Ctx.Refresh();
        }
    }

    public override void OnMouseUp(Coordinates coords)
    {
        if (Start is null) return;
        var end = MaybeSnap(coords);
        var r = Ctx.FormsPlot.Plot.Add.Rectangle(
            Math.Min(Start.Value.X, end.X),
            Math.Max(Start.Value.X, end.X),
            Math.Min(Start.Value.Y, end.Y),
            Math.Max(Start.Value.Y, end.Y));
        r.LineWidth = 2;
        r.LineColor = Colors.Purple;
        r.FillColor = Colors.Purple.WithAlpha(0.1);
        Ctx.AddFinal(r);
        Ctx.SetPreview(null);
        Preview = null;
        Start = null;
        Ctx.Refresh();
    }
}
