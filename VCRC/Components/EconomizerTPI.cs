using System;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.Units;
using VCRC.Components.Validators;

namespace VCRC.Components;

/// <summary>
///     Economizer as a component of VCRC with two-phase injection to the compressor.
/// </summary>
public record EconomizerTPI
{
    /// <summary>
    ///     Economizer as a component of VCRC with two-phase injection to the compressor.
    /// </summary>
    /// <param name="pressure">Absolute intermediate pressure.</param>
    /// <param name="temperatureDifference">Temperature difference at economizer "cold" side.</param>
    /// <exception cref="ValidationException">
    ///     Temperature difference at the economizer 'cold' side should be in [0;50] K!
    /// </exception>
    public EconomizerTPI(Pressure pressure, TemperatureDelta temperatureDifference)
    {
        (Pressure, TemperatureDifference) =
            (pressure.ToUnit(PressureUnit.Kilopascal),
                temperatureDifference.ToUnit(TemperatureDeltaUnit.Kelvin));
        new EconomizerTPIValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Economizer as a component of VCRC with two-phase injection.
    /// </summary>
    /// <remarks>
    ///     The intermediate pressure is calculated as the square root of the product
    ///     of the evaporating pressure and the condensing or gas cooler pressure.
    /// </remarks>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="heatEmitter">Condenser or gas cooler.</param>
    /// <param name="temperatureDifference">Temperature difference at economizer "cold" side.</param>
    /// <exception cref="ValidationException">
    ///     Temperature difference at the economizer 'cold' side should be in [0;50] K!
    /// </exception>
    public EconomizerTPI(Evaporator evaporator, IHeatEmitter heatEmitter, TemperatureDelta temperatureDifference) :
        this(Math.Sqrt(evaporator.Pressure.Pascals * heatEmitter.Pressure.Pascals).Pascals(), temperatureDifference)
    {
    }

    /// <summary>
    ///     Temperature difference at economizer "cold" side.
    /// </summary>
    public TemperatureDelta TemperatureDifference { get; }

    /// <summary>
    ///     Absolute intermediate pressure.
    /// </summary>
    public Pressure Pressure { get; }
}