using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

public abstract class BaseDrawStrategy : IDrawModeStrategy
{
    protected readonly IInteractionContext Ctx;

    protected Coordinates? Start;
    protected IPlottable? Preview;

    protected BaseDrawStrategy(IInteractionContext ctx) => Ctx = ctx;

    public virtual void Activate() { }

    public virtual void Deactivate()
    {
        if (Preview is not null)
            Ctx.SetPreview(null);
        Preview = null;
        Start = null;
        Ctx.Refresh();
    }

    public abstract void OnMouseDown(Coordinates coords);
    public abstract void OnMouseMove(Coordinates coords);
    public abstract void OnMouseUp(Coordinates coords);

    protected Coordinates MaybeSnap(Coordinates c) => Ctx.ApplySnap(c);

    // Default implementations for interface methods (not used in current architecture but required by interface)
    public abstract IPlottable CreatePreview(Coordinates start, Coordinates end, Plot plot);
    public abstract IPlottable CreateFinal(Coordinates start, Coordinates end, Plot plot);
}
