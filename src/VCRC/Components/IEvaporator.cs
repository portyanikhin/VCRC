namespace VCRC;

/// <summary>
///     Evaporator.
/// </summary>
public interface IEvaporator : IMainHeatExchanger
{
    /// <summary>
    ///     Superheat.
    /// </summary>
    public TemperatureDelta Superheat { get; }
}
