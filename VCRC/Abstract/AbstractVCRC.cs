using FluentValidation;
using SharpProp;
using UnitsNet;
using VCRC.Abstract.Validators;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Abstract;

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
    /// <param name="heatEmitter">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    protected AbstractVCRC(Evaporator evaporator, Compressor compressor, IHeatEmitter heatEmitter)
    {
        (Evaporator, Compressor, HeatEmitter) =
            (evaporator, compressor, heatEmitter);
        new AbstractVCRCValidator().ValidateAndThrow(this);
        Refrigerant = new Refrigerant(Evaporator.RefrigerantName);
        Point1 = Evaporator.Superheat == TemperatureDelta.Zero
            ? Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
                Input.Quality(TwoPhase.Dew.VaporQuality()))
            : Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
                Input.Temperature(Evaporator.Temperature + Evaporator.Superheat));
    }

    internal IHeatEmitter HeatEmitter { get; }

    protected Refrigerant Refrigerant { get; }

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
    public double EER => SpecificCoolingCapacity / SpecificWork;

    /// <summary>
    ///     Coefficient of performance, aka heating coefficient (dimensionless).
    /// </summary>
    public double COP => SpecificHeatingCapacity / SpecificWork;
}