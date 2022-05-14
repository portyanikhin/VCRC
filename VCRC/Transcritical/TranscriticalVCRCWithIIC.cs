using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;
using VCRC.Fluids;

namespace VCRC.Transcritical;

/// <summary>
///     Two-stage transcritical VCRC with incomplete intercooling.
/// </summary>
public class TranscriticalVCRCWithIIC : AbstractVCRCWithIIC
{
    /// <summary>
    ///     Two-stage transcritical VCRC with incomplete intercooling.
    /// </summary>
    /// <remarks>
    ///     If an intermediate vessel is not specified, it will be constructed automatically.
    /// </remarks>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="gasCooler">Gas cooler.</param>
    /// <param name="intermediateVessel">Intermediate vessel (optional).</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Refrigerant should not have a temperature glide!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be greater than evaporating pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be less than gas cooler pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     There should be a two-phase refrigerant at the intermediate vessel inlet!
    /// </exception>
    public TranscriticalVCRCWithIIC(Evaporator evaporator, Compressor compressor, GasCooler gasCooler,
        IntermediateVessel? intermediateVessel = null) :
        base(evaporator, compressor, gasCooler, intermediateVessel) =>
        GasCooler = gasCooler;

    /// <summary>
    ///     Gas cooler as a transcritical VCRC component.
    /// </summary>
    public GasCooler GasCooler { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge / gas cooler inlet.
    /// </summary>
    public new Refrigerant Point4 => base.Point4;

    /// <summary>
    ///     Point 5 – gas cooler outlet / first EV inlet.
    /// </summary>
    public new Refrigerant Point5 => base.Point5;
}