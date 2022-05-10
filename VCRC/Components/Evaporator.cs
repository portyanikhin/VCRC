using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.Units;
using VCRC.Components.Validators;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Components;

/// <summary>
///     Evaporator as a VCRC component.
/// </summary>
public record Evaporator
{
    /// <summary>
    ///     Evaporator as a VCRC component.
    /// </summary>
    /// <param name="refrigerantName">Selected refrigerant name.</param>
    /// <param name="temperature">Evaporating temperature (dew-point).</param>
    /// <param name="superheat">Superheat in the evaporator.</param>
    /// <exception cref="ValidationException">
    ///     Evaporating temperature should be in ({TripleTemperature};{CriticalTemperature}) °C!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Superheat in the evaporator should be in [0;50] K!
    /// </exception>
    public Evaporator(FluidsList refrigerantName, Temperature temperature, TemperatureDelta superheat)
    {
        (RefrigerantName, Temperature, Superheat) =
            (refrigerantName, temperature.ToUnit(TemperatureUnit.DegreeCelsius),
                superheat.ToUnit(TemperatureDeltaUnit.Kelvin));
        new EvaporatorValidator(new Refrigerant(RefrigerantName)).ValidateAndThrow(this);
    }

    /// <summary>
    ///     Selected refrigerant name.
    /// </summary>
    public FluidsList RefrigerantName { get; }

    /// <summary>
    ///     Evaporating temperature (dew-point).
    /// </summary>
    public Temperature Temperature { get; }

    /// <summary>
    ///     Superheat in the evaporator.
    /// </summary>
    public TemperatureDelta Superheat { get; }

    /// <summary>
    ///     Absolute evaporating pressure.
    /// </summary>
    public Pressure Pressure =>
        new Refrigerant(RefrigerantName)
            .WithState(Input.Temperature(Temperature),
                Input.Quality(TwoPhase.Dew.VaporQuality())).Pressure;
}