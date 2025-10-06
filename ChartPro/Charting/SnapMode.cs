namespace ChartPro.Charting;

/// <summary>
/// Defines the snap/magnet mode for drawing tools.
/// </summary>
public enum SnapMode
{
    /// <summary>
    /// No snapping applied.
    /// </summary>
    None,

    /// <summary>
    /// Snap to rounded price levels (nice numbers).
    /// </summary>
    Price,

    /// <summary>
    /// Snap to nearest candle's OHLC values.
    /// </summary>
    CandleOHLC
}
