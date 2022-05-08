using System.Diagnostics.CodeAnalysis;
using SharpProp;
using UnitsNet;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC;

/// <summary>
///     VCRC base class.
/// </summary>
public abstract class AbstractVCRC
{
    /// <summary>
    ///     VCRC base class.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    protected AbstractVCRC(Evaporator evaporator, Compressor compressor)
    {
        (Evaporator, Compressor) = (evaporator, compressor);
        RefrigerantName = Evaporator.RefrigerantName;
        Refrigerant = new Refrigerant(RefrigerantName);
        Point1 = Evaporator.Superheat == TemperatureDelta.Zero
            ? Evaporator.DewPoint.Clone()
            : Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
                Input.Temperature(Evaporator.DewPoint.Temperature + Evaporator.Superheat));
    }

    protected Refrigerant Refrigerant { get; }

    /// <summary>
    ///     Selected refrigerant name.
    /// </summary>
    public FluidsList RefrigerantName { get; }

    /// <summary>
    ///     Evaporator as a VCRC component.
    /// </summary>
    public Evaporator Evaporator { get; }

    /// <summary>
    ///     Compressor as a VCRC component.
    /// </summary>
    public Compressor Compressor { get; }

    /// <summary>
    ///     Point 1 – evaporator outlet.
    /// </summary>
    public Refrigerant Point1 { get; }

    /// <summary>
    ///     Specific work of isentropic compression (by default, kJ/kg).
    /// </summary>
    public SpecificEnergy IsentropicSpecificWork { get; protected init; }

    /// <summary>
    ///     Specific work of real compression (by default, kJ/kg).
    /// </summary>
    public SpecificEnergy SpecificWork { get; protected init; }

    /// <summary>
    ///     Specific cooling capacity of the cycle (by default, kJ/kg).
    /// </summary>
    public SpecificEnergy SpecificCoolingCapacity { get; protected init; }

    /// <summary>
    ///     Specific heating capacity of the cycle (by default, kJ/kg).
    /// </summary>
    public SpecificEnergy SpecificHeatingCapacity { get; protected init; }

    /// <summary>
    ///     Energy efficiency ratio, aka cooling coefficient (dimensionless).
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public double EER => SpecificCoolingCapacity / SpecificWork;

    /// <summary>
    ///     Coefficient of performance, aka heating coefficient (dimensionless).
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public double COP => SpecificHeatingCapacity / SpecificWork;
}