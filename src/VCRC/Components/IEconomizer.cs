namespace VCRC;

/// <summary>
///     Economizer.
/// </summary>
public interface IEconomizer : IAuxiliaryHeatExchanger
{
    /// <summary>
    ///     Superheat.
    /// </summary>
    TemperatureDelta Superheat { get; }
}
