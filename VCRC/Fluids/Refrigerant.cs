using System;
using CoolProp;
using FluentValidation;
using SharpProp;
using UnitsNet;
using VCRC.Validators;

namespace VCRC.Fluids;

/// <summary>
///     VCRC working fluid.
/// </summary>
public class Refrigerant : Fluid
{
    /// <summary>
    ///     VCRC working fluid.
    /// </summary>
    /// <param name="name">Selected refrigerant.</param>
    /// <exception cref="ValidationException">
    ///     The selected fluid is not a refrigerant (its name should start with 'R')!
    /// </exception>
    public Refrigerant(FluidsList name) : base(name) => new RefrigerantValidator().ValidateAndThrow(this);

    /// <summary>
    ///     Absolute pressure at the critical point (by default, kPa).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid critical pressure!</exception>
    public new Pressure CriticalPressure =>
        base.CriticalPressure ?? throw new NullReferenceException("Invalid critical pressure!");

    /// <summary>
    ///     Temperature at the critical point (by default, °C).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid critical temperature!</exception>
    public new Temperature CriticalTemperature =>
        base.CriticalTemperature ?? throw new NullReferenceException("Invalid critical temperature!");

    /// <summary>
    ///     Absolute pressure at the triple point (by default, kPa).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid triple pressure!</exception>
    public new Pressure TriplePressure =>
        base.TriplePressure ?? throw new NullReferenceException("Invalid triple pressure!");

    /// <summary>
    ///     Temperature at the triple point (by default, °C).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid triple temperature!</exception>
    public new Temperature TripleTemperature =>
        base.TripleTemperature ?? throw new NullReferenceException("Invalid triple temperature!");

    public override Refrigerant Factory() => new(Name);

    public override Refrigerant
        WithState(IKeyedInput<Parameters> firstInput, IKeyedInput<Parameters> secondInput) =>
        (Refrigerant) base.WithState(firstInput, secondInput);
}