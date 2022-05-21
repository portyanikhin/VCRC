namespace VCRC;

/// <summary>
///     Two-phase points.
/// </summary>
public enum TwoPhase
{
    /// <summary>
    ///     Bubble-point.
    /// </summary>
    [VaporQuality(0)] Bubble,

    /// <summary>
    ///     Dew-point.
    /// </summary>
    [VaporQuality(100)] Dew
}