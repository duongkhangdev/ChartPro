namespace ChartPro.Charting.Interactions.Strategies;

public class DrawModeStrategyFactory
{
    private readonly IInteractionContext _ctx;

    public DrawModeStrategyFactory(IInteractionContext ctx) => _ctx = ctx;

    public IDrawModeStrategy Create(ChartDrawMode mode) => mode switch
    {
        ChartDrawMode.TrendLine => new TrendLineStrategy(_ctx),
        ChartDrawMode.HorizontalLine => new HorizontalLineStrategy(_ctx),
        ChartDrawMode.VerticalLine => new VerticalLineStrategy(_ctx),
        ChartDrawMode.Rectangle => new RectangleStrategy(_ctx),
        ChartDrawMode.Circle => new CircleStrategy(_ctx),
        ChartDrawMode.FibonacciRetracement => new FibonacciRetracementStrategy(_ctx),
        _ => new NoopStrategy(_ctx),
    };

    private sealed class NoopStrategy : BaseDrawStrategy
    {
        public NoopStrategy(IInteractionContext ctx) : base(ctx) { }
        public override void OnMouseDown(ScottPlot.Coordinates coords) { }
        public override void OnMouseMove(ScottPlot.Coordinates coords) { }
        public override void OnMouseUp(ScottPlot.Coordinates coords) { }
    }
}
