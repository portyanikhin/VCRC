namespace VCRC;

/// <summary>
///     Economizer.
/// </summary>
public interface IEconomizer : IAuxiliaryHeatExchanger
{
    /// <summary>
    ///     Superheat.
    /// </summary>
    public TemperatureDelta Superheat { get; }
}
