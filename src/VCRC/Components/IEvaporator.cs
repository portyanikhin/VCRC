namespace VCRC;

/// <summary>
///     Evaporator.
/// </summary>
public interface IEvaporator : IMainHeatExchanger
{
    /// <summary>
    ///     Superheat.
    /// </summary>
    TemperatureDelta Superheat { get; }
}
