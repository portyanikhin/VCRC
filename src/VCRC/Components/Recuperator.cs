namespace VCRC;

/// <summary>
///     Recuperator.
/// </summary>
public record Recuperator : IAuxiliaryHeatExchanger
{
    /// <summary>
    ///     Recuperator.
    /// </summary>
    /// <param name="temperatureDifference">
    ///     Temperature difference at the "hot" side.
    /// </param>
    /// <exception cref="ValidationException">
    ///     Temperature difference at the recuperator 'hot' side
    ///     should be in (0;50) K!
    /// </exception>
    public Recuperator(TemperatureDelta temperatureDifference)
    {
        TemperatureDifference = temperatureDifference.ToUnit(
            TemperatureDeltaUnit.Kelvin
        );
        new RecuperatorValidator().ValidateAndThrow(this);
    }

    public TemperatureDelta TemperatureDifference { get; }
}
