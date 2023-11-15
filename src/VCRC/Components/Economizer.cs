namespace VCRC;

/// <inheritdoc cref="IEconomizer"/>
public record Economizer : EconomizerWithTPI, IEconomizer
{
    /// <inheritdoc cref="Economizer"/>
    /// <param name="temperatureDifference">
    ///     Temperature difference at the "cold" side.
    /// </param>
    /// <param name="superheat">Superheat.</param>
    /// <exception cref="ValidationException">
    ///     Temperature difference at the economizer 'cold' side
    ///     should be in (0;50) K!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Superheat in the economizer should be in [0;50] K!
    /// </exception>
    public Economizer(
        TemperatureDelta temperatureDifference,
        TemperatureDelta superheat
    )
        : base(temperatureDifference)
    {
        Superheat = superheat.ToUnit(TemperatureDeltaUnit.Kelvin);
        new EconomizerValidator().ValidateAndThrow(this);
    }

    public TemperatureDelta Superheat { get; }
}
