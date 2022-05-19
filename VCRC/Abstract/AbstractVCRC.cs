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
        HeatEmitterOutlet = HeatEmitter is Condenser condenser
            ? condenser.Subcooling == TemperatureDelta.Zero
                ? Refrigerant.WithState(Input.Pressure(condenser.Pressure),
                    Input.Quality(TwoPhase.Bubble.VaporQuality()))
                : Refrigerant.WithState(Input.Pressure(condenser.Pressure),
                    Input.Temperature(condenser.Temperature - condenser.Subcooling))
            : Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
                Input.Temperature(HeatEmitter.Temperature));
    }

    internal IHeatEmitter HeatEmitter { get; }

    protected Refrigerant Refrigerant { get; }

    protected Refrigerant HeatEmitterOutlet { get; }

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
    internal Refrigerant Point1 { get; }

    /// <summary>
    ///     Specific work of isentropic compression.
    /// </summary>
    public abstract SpecificEnergy IsentropicSpecificWork { get; }

    /// <summary>
    ///     Specific work of real compression.
    /// </summary>
    public SpecificEnergy SpecificWork =>
        IsentropicSpecificWork / Compressor.IsentropicEfficiency.DecimalFractions;

    /// <summary>
    ///     Specific cooling capacity of the cycle.
    /// </summary>
    public abstract SpecificEnergy SpecificCoolingCapacity { get; }

    /// <summary>
    ///     Specific heating capacity of the cycle.
    /// </summary>
    public abstract SpecificEnergy SpecificHeatingCapacity { get; }

    /// <summary>
    ///     Energy efficiency ratio, aka cooling coefficient.
    /// </summary>
    public double EER => SpecificCoolingCapacity / SpecificWork;

    /// <summary>
    ///     Coefficient of performance, aka heating coefficient.
    /// </summary>
    public double COP => SpecificHeatingCapacity / SpecificWork;
}