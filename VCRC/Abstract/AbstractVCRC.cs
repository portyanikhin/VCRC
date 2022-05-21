using FluentValidation;
using SharpProp;
using UnitsNet;

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
        (Condenser, GasCooler) =
            (HeatEmitter as Condenser, HeatEmitter as GasCooler);
        Refrigerant = new Refrigerant(Evaporator.RefrigerantName);
        HeatEmitterOutlet = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            HeatEmitter is Condenser condenser
                ? condenser.Subcooling == TemperatureDelta.Zero
                    ? Input.Quality(TwoPhase.Bubble.VaporQuality())
                    : Input.Temperature(condenser.Temperature - condenser.Subcooling)
                : Input.Temperature(HeatEmitter.Temperature));
        Point1 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Evaporator.Superheat == TemperatureDelta.Zero
                ? Input.Quality(TwoPhase.Dew.VaporQuality())
                : Input.Temperature(Evaporator.Temperature + Evaporator.Superheat));
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
    ///     Condenser as a subcritical VCRC component.
    /// </summary>
    public Condenser? Condenser { get; }

    /// <summary>
    ///     Gas cooler as a transcritical VCRC component.
    /// </summary>
    public GasCooler? GasCooler { get; }

    /// <summary>
    ///     <c>true</c> if transcritical VCRC,
    ///     <c>false</c> if subcritical VCRC.
    /// </summary>
    public bool IsTranscritical => GasCooler is not null;

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