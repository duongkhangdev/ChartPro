using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

public class CircleStrategy : BaseDrawStrategy
{
    public CircleStrategy(IInteractionContext ctx) : base(ctx) { }

    private static (double cx, double cy, double rx, double ry) GetEllipse(Coordinates a, Coordinates b)
    {
        var cx = (a.X + b.X) / 2;
        var cy = (a.Y + b.Y) / 2;
        var rx = Math.Abs(b.X - a.X) / 2;
        var ry = Math.Abs(b.Y - a.Y) / 2;
        return (cx, cy, rx, ry);
    }

    public override void OnMouseDown(Coordinates coords)
    {
        Start = MaybeSnap(coords);
        var (cx, cy, rx, ry) = GetEllipse(Start.Value, Start.Value);
        var e = Ctx.FormsPlot.Plot.Add.Ellipse(cx, cy, rx, ry);
        e.LineWidth = 1;
        e.LineColor = Colors.Gray.WithAlpha(0.5);
        e.FillColor = Colors.Gray.WithAlpha(0.1);
        Preview = e;
        Ctx.SetPreview(e);
        Ctx.Refresh();
    }

    public override void OnMouseMove(Coordinates coords)
    {
        if (Preview is ScottPlot.Plottables.Ellipse e && Start is Coordinates s)
        {
            var end = MaybeSnap(coords);
            var (cx, cy, rx, ry) = GetEllipse(s, end);
            e.Center = new Coordinates(cx, cy);
            e.RadiusX = rx;
            e.RadiusY = ry;
            Ctx.Refresh();
        }
    }

    public override void OnMouseUp(Coordinates coords)
    {
        if (Start is null) return;
        var end = MaybeSnap(coords);
        var (cx, cy, rx, ry) = GetEllipse(Start.Value, end);
        var e = Ctx.FormsPlot.Plot.Add.Ellipse(cx, cy, rx, ry);
        e.LineWidth = 2;
        e.LineColor = Colors.Cyan;
        e.FillColor = Colors.Cyan.WithAlpha(0.1);
        Ctx.AddFinal(e);
        Ctx.SetPreview(null);
        Preview = null;
        Start = null;
        Ctx.Refresh();
    }
}
