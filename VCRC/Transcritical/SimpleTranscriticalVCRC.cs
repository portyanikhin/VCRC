using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;
using VCRC.Fluids;

namespace VCRC.Transcritical;

/// <summary>
///     Simple single-stage transcritical VCRC.
/// </summary>
public class SimpleTranscriticalVCRC : AbstractSimpleVCRC
{
    /// <summary>
    ///     Simple single-stage transcritical VCRC.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="gasCooler">Gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    public SimpleTranscriticalVCRC(Evaporator evaporator, Compressor compressor, GasCooler gasCooler) :
        base(evaporator, compressor, gasCooler) =>
        GasCooler = gasCooler;

    /// <summary>
    ///     Gas cooler as a transcritical VCRC component.
    /// </summary>
    public GasCooler GasCooler { get; }

    /// <summary>
    ///     Point 2 – compression stage discharge / gas cooler inlet.
    /// </summary>
    public new Refrigerant Point2 => base.Point2;

    /// <summary>
    ///     Point 3 – gas cooler outlet / EV inlet.
    /// </summary>
    public new Refrigerant Point3 => base.Point3;
}