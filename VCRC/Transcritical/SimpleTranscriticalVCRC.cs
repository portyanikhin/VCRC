using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;

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
}