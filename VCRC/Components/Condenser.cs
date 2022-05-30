using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.Units;

namespace VCRC;

/// <summary>
///     Condenser as a VCRC component.
/// </summary>
public record Condenser : IHeatReleaser
{
    /// <summary>
    ///     Condenser as a VCRC component.
    /// </summary>
    /// <param name="refrigerantName">Selected refrigerant name.</param>
    /// <param name="temperature">Condensing temperature (bubble point).</param>
    /// <param name="subcooling">Subcooling in the condenser.</param>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be in ({TripleTemperature};{CriticalTemperature}) °C!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Subcooling in the condenser should be in [0;50] K!
    /// </exception>
    public Condenser(FluidsList refrigerantName, Temperature temperature, TemperatureDelta subcooling)
    {
        (RefrigerantName, Temperature, Subcooling) =
            (refrigerantName, temperature.ToUnit(TemperatureUnit.DegreeCelsius),
                subcooling.ToUnit(TemperatureDeltaUnit.Kelvin));
        new CondenserValidator(new Refrigerant(RefrigerantName)).ValidateAndThrow(this);
    }

    /// <summary>
    ///     Subcooling in the condenser.
    /// </summary>
    public TemperatureDelta Subcooling { get; }

    /// <summary>
    ///     Selected refrigerant name.
    /// </summary>
    public FluidsList RefrigerantName { get; }

    /// <summary>
    ///     Condensing temperature (bubble point).
    /// </summary>
    public Temperature Temperature { get; }

    /// <summary>
    ///     Absolute condensing pressure.
    /// </summary>
    public Pressure Pressure => Outlet.Pressure;

    /// <summary>
    ///     Condenser outlet.
    /// </summary>
    public Refrigerant Outlet =>
        new Refrigerant(RefrigerantName)
            .Subcooled(Temperature, Subcooling);
}