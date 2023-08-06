namespace VCRC;

/// <summary>
///     Economizer as a component
///     of VCRC with two-phase injection into the compressor.
/// </summary>
public record EconomizerWithTPI
{
    /// <summary>
    ///     Economizer as a component
    ///     of VCRC with two-phase injection into the compressor.
    /// </summary>
    /// <param name="temperatureDifference">
    ///     Temperature difference at economizer "cold" side.
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

    /// <summary>
    ///     Temperature difference at economizer "cold" side.
    /// </summary>
    public TemperatureDelta TemperatureDifference { get; }
}
