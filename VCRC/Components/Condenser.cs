using System;
using FluentValidation;
using SharpProp;
using UnitsNet;
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Validators.Components;

namespace VCRC.Components;

/// <summary>
///     Condenser as a VCRC component.
/// </summary>
public class Condenser : IEquatable<Condenser>
{
    /// <summary>
    ///     Condenser as a VCRC component.
    /// </summary>
    /// <param name="refrigerantName">Selected refrigerant name.</param>
    /// <param name="temperature">Condensing temperature.</param>
    /// <param name="subcooling">Subcooling in the condenser.</param>
    /// <param name="pressureDefinition">Definition of the condensing pressure (bubble-point or dew-point).</param>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be in ({TripleTemperature};{CriticalTemperature}) °C!
    /// </exception>
    /// <exception cref="ValidationException">Subcooling in the condenser should be in [0;50] K!</exception>
    public Condenser(FluidsList refrigerantName, Temperature temperature, TemperatureDelta subcooling,
        TwoPhase pressureDefinition = TwoPhase.Bubble)
    {
        RefrigerantName = refrigerantName;
        Refrigerant = new Refrigerant(RefrigerantName);
        Temperature = temperature;
        Subcooling = subcooling;
        PressureDefinition = pressureDefinition;
        new CondenserValidator(Refrigerant).ValidateAndThrow(this);
        Pressure = Refrigerant.WithState(Input.Temperature(Temperature),
            Input.Quality(PressureDefinition.VaporQuality())).Pressure;
    }

    private Refrigerant Refrigerant { get; }

    /// <summary>
    ///     Selected refrigerant name.
    /// </summary>
    internal FluidsList RefrigerantName { get; }

    /// <summary>
    ///     Condensing temperature.
    /// </summary>
    public Temperature Temperature { get; }

    /// <summary>
    ///     Subcooling in the condenser.
    /// </summary>
    public TemperatureDelta Subcooling { get; }

    /// <summary>
    ///     Definition of the condensing pressure (bubble-point or dew-point).
    /// </summary>
    public TwoPhase PressureDefinition { get; }

    /// <summary>
    ///     Absolute condensing pressure.
    /// </summary>
    public Pressure Pressure { get; }

    public bool Equals(Condenser? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetHashCode() == other.GetHashCode();
    }

    public override bool Equals(object? obj) => Equals(obj as Condenser);

    public override int GetHashCode() =>
        HashCode.Combine((int) RefrigerantName, Temperature, Subcooling, (int) PressureDefinition, Pressure);

    public static bool operator ==(Condenser? left, Condenser? right) => Equals(left, right);

    public static bool operator !=(Condenser? left, Condenser? right) => !Equals(left, right);
}