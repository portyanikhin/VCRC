﻿using System;
using FluentValidation;
using UnitsNet;
using VCRC.Components.Validators;

namespace VCRC.Components;

/// <summary>
///     Economizer as a VCRC component.
/// </summary>
public class Economizer : EconomizerTPI, IEquatable<Economizer>
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
    /// <exception cref="ValidationException">Superheat in the economizer should be in [0;50] K!</exception>
    public Economizer(Pressure pressure, TemperatureDelta temperatureDifference, TemperatureDelta superheat) :
        base(pressure, temperatureDifference)
    {
        Superheat = superheat;
        new EconomizerValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Economizer as VCRC component.
    /// </summary>
    /// <remarks>
    ///     The intermediate pressure is calculated as the square root of the product
    ///     of evaporating pressure and condensing pressure.
    /// </remarks>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="condenser">Condenser.</param>
    /// <param name="temperatureDifference">Temperature difference at economizer "cold" side.</param>
    /// <param name="superheat">Superheat in the economizer.</param>
    /// <exception cref="ValidationException">
    ///     Temperature difference at the economizer 'cold' side should be in [0;50] K!
    /// </exception>
    /// <exception cref="ValidationException">Superheat in the economizer should be in [0;50] K!</exception>
    public Economizer(Evaporator evaporator, Condenser condenser, TemperatureDelta temperatureDifference,
        TemperatureDelta superheat) : base(evaporator, condenser, temperatureDifference)
    {
        Superheat = superheat;
        new EconomizerValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Superheat in the economizer.
    /// </summary>
    public TemperatureDelta Superheat { get; }

    public bool Equals(Economizer? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetHashCode() == other.GetHashCode();
    }

    public override bool Equals(object? obj) => Equals(obj as Economizer);

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Superheat);

    public static bool operator ==(Economizer? left, Economizer? right) => Equals(left, right);

    public static bool operator !=(Economizer? left, Economizer? right) => !Equals(left, right);
}