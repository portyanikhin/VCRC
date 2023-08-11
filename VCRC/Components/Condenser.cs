namespace VCRC;

/// <summary>
///     Condenser.
/// </summary>
public record Condenser : ICondenser
{
    /// <summary>
    ///     Condenser.
    /// </summary>
    /// <param name="refrigerantName">Selected refrigerant name.</param>
    /// <param name="temperature">Condensing temperature (bubble point).</param>
    /// <param name="subcooling">Subcooling.</param>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be in
    ///     <c>({TripleTemperature};{CriticalTemperature})</c> °C!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Subcooling in the condenser should be in [0;50] K!
    /// </exception>
    public Condenser(
        FluidsList refrigerantName,
        Temperature temperature,
        TemperatureDelta subcooling
    )
    {
        RefrigerantName = refrigerantName;
        Temperature = temperature.ToUnit(TemperatureUnit.DegreeCelsius);
        Subcooling = subcooling.ToUnit(TemperatureDeltaUnit.Kelvin);
        new CondenserValidator(
            new Refrigerant(RefrigerantName)
        ).ValidateAndThrow(this);
    }

    public TemperatureDelta Subcooling { get; }

    public FluidsList RefrigerantName { get; }

    public Temperature Temperature { get; }

    public Pressure Pressure => Outlet.Pressure;

    public IRefrigerant Outlet =>
        new Refrigerant(RefrigerantName).Subcooled(Temperature, Subcooling);
}
