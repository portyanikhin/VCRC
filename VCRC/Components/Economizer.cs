using FluentValidation;
using UnitsNet;
using UnitsNet.Units;

namespace VCRC;

/// <summary>
///     Economizer as a VCRC component.
/// </summary>
public record Economizer : EconomizerWithTPI
{
    /// <summary>
    ///     Economizer as a VCRC component.
    /// </summary>
    /// <param name="temperatureDifference">Temperature difference at economizer "cold" side.</param>
    /// <param name="superheat">Superheat in the economizer.</param>
    /// <exception cref="ValidationException">
    ///     Temperature difference at the economizer 'cold' side should be in (0;50) K!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Superheat in the economizer should be in [0;50] K!
    /// </exception>
    public Economizer(TemperatureDelta temperatureDifference, TemperatureDelta superheat) :
        base(temperatureDifference)
    {
        Superheat = superheat.ToUnit(TemperatureDeltaUnit.Kelvin);
        new EconomizerValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Superheat in the economizer.
    /// </summary>
    public TemperatureDelta Superheat { get; }
}