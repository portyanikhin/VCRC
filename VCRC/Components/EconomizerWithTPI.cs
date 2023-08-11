namespace VCRC;

/// <summary>
///     Economizer with two-phase injection into the compressor.
/// </summary>
public record EconomizerWithTPI : IAuxiliaryHeatExchanger
{
    /// <summary>
    ///     Economizer with two-phase injection into the compressor.
    /// </summary>
    /// <param name="temperatureDifference">
    ///     Temperature difference at the "cold" side.
    /// </param>
    /// <exception cref="ValidationException">
    ///     Temperature difference at the economizer 'cold' side
    ///     should be in (0;50) K!
    /// </exception>
    public EconomizerWithTPI(TemperatureDelta temperatureDifference)
    {
        TemperatureDifference = temperatureDifference.ToUnit(
            TemperatureDeltaUnit.Kelvin
        );
        new EconomizerWithTPIValidator().ValidateAndThrow(this);
    }

    public TemperatureDelta TemperatureDifference { get; }
}
