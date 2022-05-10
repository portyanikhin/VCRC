using FluentValidation;
using UnitsNet;
using UnitsNet.Units;
using VCRC.Components.Validators;

namespace VCRC.Components;

/// <summary>
///     Economizer as a VCRC component.
/// </summary>
public record Economizer : EconomizerTPI
{
    /// <summary>
    ///     Economizer as a VCRC component.
    /// </summary>
    /// <param name="pressure">Absolute intermediate pressure.</param>
    /// <param name="temperatureDifference">Temperature difference at economizer "cold" side.</param>
    /// <param name="superheat">Superheat in the economizer.</param>
    /// <exception cref="ValidationException">
    ///     Temperature difference at the economizer 'cold' side should be in [0;50] K!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Superheat in the economizer should be in [0;50] K!
    /// </exception>
    public Economizer(Pressure pressure, TemperatureDelta temperatureDifference, TemperatureDelta superheat) :
        base(pressure, temperatureDifference)
    {
        Superheat = superheat.ToUnit(TemperatureDeltaUnit.Kelvin);
        new EconomizerValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Economizer as VCRC component.
    /// </summary>
    /// <remarks>
    ///     The intermediate pressure is calculated as the square root of the product
    ///     of the evaporating pressure and the condensing or gas cooler pressure.
    /// </remarks>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="heatEmitter">Condenser or gas cooler.</param>
    /// <param name="temperatureDifference">Temperature difference at economizer "cold" side.</param>
    /// <param name="superheat">Superheat in the economizer.</param>
    /// <exception cref="ValidationException">
    ///     Temperature difference at the economizer 'cold' side should be in [0;50] K!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Superheat in the economizer should be in [0;50] K!
    /// </exception>
    public Economizer(Evaporator evaporator, IHeatEmitter heatEmitter, TemperatureDelta temperatureDifference,
        TemperatureDelta superheat) : base(evaporator, heatEmitter, temperatureDifference)
    {
        Superheat = superheat.ToUnit(TemperatureDeltaUnit.Kelvin);
        new EconomizerValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Superheat in the economizer.
    /// </summary>
    public TemperatureDelta Superheat { get; }
}