namespace VCRC;

/// <inheritdoc cref="IEvaporator"/>
public record Evaporator : IEvaporator
{
    /// <inheritdoc cref="Evaporator"/>
    /// <param name="refrigerantName">Selected refrigerant name.</param>
    /// <param name="temperature">Evaporating temperature (dew point).</param>
    /// <param name="superheat">Superheat.</param>
    /// <exception cref="ValidationException">
    ///     Evaporating temperature should be in
    ///     <c>({TripleTemperature};{CriticalTemperature})</c> °C!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Superheat in the evaporator should be in [0;50] K!
    /// </exception>
    public Evaporator(
        FluidsList refrigerantName,
        Temperature temperature,
        TemperatureDelta superheat
    )
    {
        RefrigerantName = refrigerantName;
        Temperature = temperature.ToUnit(TemperatureUnit.DegreeCelsius);
        Superheat = superheat.ToUnit(TemperatureDeltaUnit.Kelvin);
        new EvaporatorValidator(
            new Refrigerant(RefrigerantName)
        ).ValidateAndThrow(this);
    }

    public FluidsList RefrigerantName { get; }

    public Temperature Temperature { get; }

    public TemperatureDelta Superheat { get; }

    public Pressure Pressure => Outlet.Pressure;

    public IRefrigerant Outlet =>
        new Refrigerant(RefrigerantName).Superheated(Temperature, Superheat);
}
