using ScottPlot;

namespace ChartPro.Charting.Interactions.Strategies;

public interface IDrawModeStrategy
{
    void Activate();
    void OnMouseDown(Coordinates coords);
    void OnMouseMove(Coordinates coords);
    void OnMouseUp(Coordinates coords);
    void Deactivate();
}
