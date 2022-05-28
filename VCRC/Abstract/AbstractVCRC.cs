using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;

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
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    protected AbstractVCRC(Evaporator evaporator, Compressor compressor, IHeatReleaser heatReleaser)
    {
        (Evaporator, Compressor, HeatReleaser) =
            (evaporator, compressor, heatReleaser);
        new AbstractVCRCValidator().ValidateAndThrow(this);
        (Condenser, GasCooler) =
            (HeatReleaser as Condenser, HeatReleaser as GasCooler);
        Refrigerant = new Refrigerant(Evaporator.RefrigerantName);
    }

    internal IHeatReleaser HeatReleaser { get; }

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
    ///     Specific mass flow rate of the evaporator.
    /// </summary>
    public Ratio EvaporatorSpecificMassFlow { get; } = 100.Percent();

    /// <summary>
    ///     Specific mass flow rate of the condenser or gas cooler.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public abstract Ratio HeatReleaserSpecificMassFlow { get; }

    /// <summary>
    ///     Specific work of isentropic compression.
    /// </summary>
    public abstract SpecificEnergy IsentropicSpecificWork { get; }

    /// <summary>
    ///     Specific work of real compression.
    /// </summary>
    public SpecificEnergy SpecificWork =>
        IsentropicSpecificWork / Compressor.Efficiency.DecimalFractions;

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