using ScottPlot;
using ScottPlot.WinForms;

namespace ChartPro.Charting.Interactions.Strategies;

public interface IInteractionContext
{
    FormsPlot FormsPlot { get; }
    bool SnapEnabled { get; }
    SnapMode SnapMode { get; }
    IReadOnlyList<OHLC>? BoundCandles { get; }

    Coordinates ApplySnap(Coordinates coords);
    void SetPreview(IPlottable? plottable);
    void AddFinal(IPlottable plottable);
    void Refresh();
}
