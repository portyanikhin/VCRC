using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC;

/// <summary>
///     Gas cooler as a transcritical VCRC component.
/// </summary>
public record GasCooler : IHeatReleaser
{
    /// <summary>
    ///     Gas cooler as a transcritical VCRC component.
    /// </summary>
    /// <remarks>
    ///     For R744, the absolute pressure in the gas cooler is optional.
    ///     If it is not specified, then the optimal pressure will be calculated automatically
    ///     in accordance with this literary source:
    ///     Yang L. et al. Minimizing COP loss from optional high pressure correlation for transcritical CO2 cycle //
    ///     Applied Thermal Engineering. – 2015. – V. 89. – P. 656-662.
    /// </remarks>
    /// <param name="refrigerantName">Selected refrigerant name.</param>
    /// <param name="outletTemperature">Gas cooler outlet temperature.</param>
    /// <param name="pressure">Gas cooler absolute pressure (optional for R744).</param>
    /// <exception cref="ArgumentException">
    ///     It is impossible to automatically calculate the absolute pressure in the gas cooler!
    ///     It is necessary to define it.
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Gas cooler outlet temperature should be greater than {CriticalTemperature} °C!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Gas cooler absolute pressure should be greater than {CriticalPressure} MPa!
    /// </exception>
    public GasCooler(FluidsList refrigerantName, Temperature outletTemperature, Pressure? pressure = null)
    {
        (RefrigerantName, Temperature) =
            (refrigerantName, outletTemperature.ToUnit(TemperatureUnit.DegreeCelsius));
        if (pressure.HasValue)
            Pressure = pressure.Value.ToUnit(PressureUnit.Kilopascal);
        else if (RefrigerantName is FluidsList.R744 && Temperature <= 60.DegreesCelsius())
            Pressure = (2.759 * Temperature.DegreesCelsius - 9.912).Bars()
                .ToUnit(PressureUnit.Kilopascal);
        else
            throw new ArgumentException(
                "It is impossible to automatically calculate the absolute pressure in the gas cooler! " +
                "It is necessary to define it.");
        new GasCoolerValidator(new Refrigerant(RefrigerantName)).ValidateAndThrow(this);
    }

    /// <summary>
    ///     Selected refrigerant name.
    /// </summary>
    public FluidsList RefrigerantName { get; }

    /// <summary>
    ///     Gas cooler outlet temperature.
    /// </summary>
    public Temperature Temperature { get; }

    /// <summary>
    ///     Gas cooler absolute pressure.
    /// </summary>
    public Pressure Pressure { get; }

    /// <summary>
    ///     Gas cooler outlet.
    /// </summary>
    public Refrigerant Outlet =>
        new Refrigerant(RefrigerantName)
            .WithState(Input.Pressure(Pressure),
                Input.Temperature(Temperature));
}