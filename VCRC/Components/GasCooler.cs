using System;
using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToTemperature;
using UnitsNet.Units;
using VCRC.Fluids;
using VCRC.Validators.Components;

namespace VCRC.Components;

/// <summary>
///     Gas cooler as a transcritical VCRC component.
/// </summary>
public class GasCooler : IEquatable<GasCooler>
{
    /// <summary>
    ///     Gas cooler as a transcritical VCRC component.
    /// </summary>
    /// <remarks>
    ///     For R744, the absolute pressure in the gas cooler is optional.
    ///     If it is not specified, then the optimal pressure will be calculated automatically
    ///     in accordance with this literary source:
    ///     Yang L. et al. Minimizing COP loss from optional high pressure correlation for transcritical CO2 cycle //
    ///     Applied Thermal Engineering. – 2015. – V. 89. – P. 656-662.
    /// </remarks>
    /// <param name="refrigerantName">Selected refrigerant name.</param>
    /// <param name="outletTemperature">Gas cooler outlet temperature.</param>
    /// <param name="pressure">Gas cooler absolute pressure (optional for R744).</param>
    /// <exception cref="ValidationException">
    ///     Gas cooler outlet temperature should be in ({CriticalTemperature};{MaxTemperature}) °C!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     It is impossible to automatically calculate the absolute pressure in the gas cooler!
    ///     It is necessary to define it.
    /// </exception>
    public GasCooler(FluidsList refrigerantName, Temperature outletTemperature, Pressure? pressure = null)
    {
        RefrigerantName = refrigerantName;
        Refrigerant = new Refrigerant(RefrigerantName);
        OutletTemperature = outletTemperature;
        new GasCoolerValidator(Refrigerant).ValidateAndThrow(this);
        if (pressure.HasValue)
            Pressure = pressure.Value;
        else if (RefrigerantName is FluidsList.R744 && OutletTemperature <= 60.DegreesCelsius())
            Pressure = (2.759 * OutletTemperature.DegreesCelsius - 9.912).Bars().ToUnit(PressureUnit.Kilopascal);
        else
            throw new ArgumentException(
                "It is impossible to automatically calculate the absolute pressure in the gas cooler! " +
                "It is necessary to define it.");
    }

    private Refrigerant Refrigerant { get; }

    /// <summary>
    ///     Selected refrigerant name.
    /// </summary>
    internal FluidsList RefrigerantName { get; }

    /// <summary>
    ///     Gas cooler outlet temperature.
    /// </summary>
    public Temperature OutletTemperature { get; }

    /// <summary>
    ///     Gas cooler absolute pressure.
    /// </summary>
    public Pressure Pressure { get; }

    public bool Equals(GasCooler? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetHashCode() == other.GetHashCode();
    }

    public override bool Equals(object? obj) => Equals(obj as GasCooler);

    public override int GetHashCode() =>
        HashCode.Combine((int) RefrigerantName, OutletTemperature, Pressure);

    public static bool operator ==(GasCooler? left, GasCooler? right) => Equals(left, right);

    public static bool operator !=(GasCooler? left, GasCooler? right) => !Equals(left, right);
}