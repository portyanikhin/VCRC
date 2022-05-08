using FluentValidation;
using VCRC.Components;
using VCRC.Transcritical.Validators;

namespace VCRC.Transcritical;

/// <summary>
///     Transcritical VCRC.
/// </summary>
public abstract class TranscriticalVCRC : AbstractVCRC
{
    /// <summary>
    ///     Transcritical VCRC.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="gasCooler">Gas cooler.</param>
    /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
    protected TranscriticalVCRC(Evaporator evaporator, Compressor compressor, GasCooler gasCooler) :
        base(evaporator, compressor)
    {
        GasCooler = gasCooler;
        new TranscriticalVCRCValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Gas cooler as a transcritical VCRC component.
    /// </summary>
    public GasCooler GasCooler { get; }
}