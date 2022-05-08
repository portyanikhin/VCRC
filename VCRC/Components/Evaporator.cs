using System;
using FluentValidation;
using SharpProp;
using UnitsNet;
using VCRC.Components.Validators;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Components;

/// <summary>
///     Evaporator as a VCRC component.
/// </summary>
public class Evaporator : IEquatable<Evaporator>
{
    /// <summary>
    ///     Evaporator as a VCRC component.
    /// </summary>
    /// <param name="refrigerantName">Selected refrigerant name.</param>
    /// <param name="temperature">Evaporating temperature.</param>
    /// <param name="superheat">Superheat in the evaporator.</param>
    /// <param name="pressureDefinition">Definition of the evaporating pressure (bubble-point or dew-point).</param>
    /// <exception cref="ValidationException">
    ///     Evaporating temperature should be in ({TripleTemperature};{CriticalTemperature}) °C!
    /// </exception>
    /// <exception cref="ValidationException">Superheat in the evaporator should be in [0;50] K!</exception>
    public Evaporator(FluidsList refrigerantName, Temperature temperature, TemperatureDelta superheat,
        TwoPhase pressureDefinition = TwoPhase.Dew)
    {
        (RefrigerantName, Temperature, Superheat, PressureDefinition) =
            (refrigerantName, temperature, superheat, pressureDefinition);
        Refrigerant = new Refrigerant(RefrigerantName);
        new EvaporatorValidator(Refrigerant).ValidateAndThrow(this);
        Pressure = Refrigerant.WithState(Input.Temperature(Temperature),
            Input.Quality(PressureDefinition.VaporQuality())).Pressure;
        DewPoint = Refrigerant.WithState(Input.Pressure(Pressure),
            Input.Quality(TwoPhase.Dew.VaporQuality()));
    }

    private Refrigerant Refrigerant { get; }

    /// <summary>
    ///     Selected refrigerant name.
    /// </summary>
    internal FluidsList RefrigerantName { get; }

    /// <summary>
    ///     Evaporating temperature.
    /// </summary>
    public Temperature Temperature { get; }

    /// <summary>
    ///     Superheat in the evaporator.
    /// </summary>
    public TemperatureDelta Superheat { get; }

    /// <summary>
    ///     Definition of the evaporating pressure (bubble-point or dew-point).
    /// </summary>
    public TwoPhase PressureDefinition { get; }

    /// <summary>
    ///     Absolute evaporating pressure.
    /// </summary>
    public Pressure Pressure { get; }

    /// <summary>
    ///     Dew-point on the evaporating isobar.
    /// </summary>
    public Refrigerant DewPoint { get; }

    public bool Equals(Evaporator? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetHashCode() == other.GetHashCode();
    }

    public override bool Equals(object? obj) => Equals(obj as Evaporator);

    public override int GetHashCode() =>
        HashCode.Combine((int) RefrigerantName, Temperature, Superheat, (int) PressureDefinition, Pressure);

    public static bool operator ==(Evaporator? left, Evaporator? right) => Equals(left, right);

    public static bool operator !=(Evaporator? left, Evaporator? right) => !Equals(left, right);
}