namespace VCRC;

/// <summary>
///     Recuperator as a VCRC component.
/// </summary>
public record Recuperator
{
    /// <summary>
    ///     Recuperator as a VCRC component.
    /// </summary>
    /// <param name="temperatureDifference">Temperature difference at recuperator "hot" side.</param>
    /// <exception cref="ValidationException">
    ///     Temperature difference at recuperator 'hot' side should be in (0;50) K!
    /// </exception>
    public Recuperator(TemperatureDelta temperatureDifference)
    {
        TemperatureDifference =
            temperatureDifference.ToUnit(TemperatureDeltaUnit.Kelvin);
        new RecuperatorValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Temperature difference at recuperator "hot" side.
    /// </summary>
    public TemperatureDelta TemperatureDifference { get; }
}