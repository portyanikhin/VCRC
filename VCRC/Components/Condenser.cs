﻿using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.Units;
using VCRC.Components.Validators;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Components;

/// <summary>
///     Condenser as a VCRC component.
/// </summary>
public record Condenser
{
    /// <summary>
    ///     Condenser as a VCRC component.
    /// </summary>
    /// <param name="refrigerantName">Selected refrigerant name.</param>
    /// <param name="temperature">Condensing temperature (bubble-point).</param>
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
    ///     Selected refrigerant name.
    /// </summary>
    public FluidsList RefrigerantName { get; }

    /// <summary>
    ///     Condensing temperature (bubble-point).
    /// </summary>
    public Temperature Temperature { get; }

    /// <summary>
    ///     Subcooling in the condenser.
    /// </summary>
    public TemperatureDelta Subcooling { get; }

    /// <summary>
    ///     Absolute condensing pressure.
    /// </summary>
    public Pressure Pressure =>
        new Refrigerant(RefrigerantName)
            .WithState(Input.Temperature(Temperature),
                Input.Quality(TwoPhase.Bubble.VaporQuality())).Pressure;
}