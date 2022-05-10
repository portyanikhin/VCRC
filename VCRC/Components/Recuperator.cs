using FluentValidation;
using UnitsNet;
using UnitsNet.Units;
using VCRC.Components.Validators;

namespace VCRC.Components;

/// <summary>
///     Recuperator as a VCRC component.
/// </summary>
public record Recuperator
{
    /// <summary>
    ///     Recuperator as a VCRC component.
    /// </summary>
    /// <param name="superheat">Superheat in the recuperator.</param>
    /// <exception cref="ValidationException">
    ///     Superheat in the recuperator should be in [0;50] K!
    /// </exception>
    public Recuperator(TemperatureDelta superheat)
    {
        Superheat = superheat.ToUnit(TemperatureDeltaUnit.Kelvin);
        new RecuperatorValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Superheat in the recuperator.
    /// </summary>
    public TemperatureDelta Superheat { get; }
}