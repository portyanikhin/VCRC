namespace VCRC;

/// <summary>
///     Auxiliary heat exchanger (economizer or recuperator).
/// </summary>
public interface IAuxiliaryHeatExchanger
{
    /// <summary>
    ///     Characteristic temperature difference
    ///     (at the "cold" side for the economizer;
    ///     at the "hot" side for the recuperator).
    /// </summary>
    public TemperatureDelta TemperatureDifference { get; }
}
