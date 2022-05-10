using System;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.Units;

namespace VCRC.Components;

/// <summary>
///     Intermediate vessel as a VCRC component.
/// </summary>
public record IntermediateVessel
{
    /// <summary>
    ///     Intermediate vessel as a VCRC component.
    /// </summary>
    /// <param name="pressure">Absolute intermediate pressure.</param>
    public IntermediateVessel(Pressure pressure) =>
        Pressure = pressure.ToUnit(PressureUnit.Kilopascal);

    /// <summary>
    ///     Intermediate vessel as a VCRC component.
    /// </summary>
    /// <remarks>
    ///     The intermediate pressure is calculated as the square root of the product
    ///     of the evaporating pressure and the condensing or gas cooler pressure.
    /// </remarks>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="heatEmitter">Condenser or gas cooler.</param>
    public IntermediateVessel(Evaporator evaporator, IHeatEmitter heatEmitter) =>
        Pressure = Math.Sqrt(evaporator.Pressure.Pascals * heatEmitter.Pressure.Pascals).Pascals()
            .ToUnit(PressureUnit.Kilopascal);

    /// <summary>
    ///     Absolute intermediate pressure.
    /// </summary>
    public Pressure Pressure { get; }
}