using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;
using VCRC.Fluids;

namespace VCRC.Transcritical;

/// <summary>
///     Single-stage transcritical VCRC with recuperator.
/// </summary>
public class TranscriticalVCRCWithRecuperator : AbstractVCRCWithRecuperator
{
    /// <summary>
    ///     Single-stage transcritical VCRC with recuperator.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="recuperator">Recuperator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="gasCooler">Gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at recuperator 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at recuperator 'cold' side!
    /// </exception>
    public TranscriticalVCRCWithRecuperator(Evaporator evaporator, Recuperator recuperator, Compressor compressor,
        GasCooler gasCooler) : base(evaporator, recuperator, compressor, gasCooler) =>
        GasCooler = gasCooler;

    /// <summary>
    ///     Gas cooler as a transcritical VCRC component.
    /// </summary>
    public GasCooler GasCooler { get; }

    /// <summary>
    ///     Point 3 – compression stage discharge / gas cooler inlet.
    /// </summary>
    public new Refrigerant Point3 => base.Point3;

    /// <summary>
    ///     Point 4 – gas cooler outlet / recuperator "hot" inlet.
    /// </summary>
    public new Refrigerant Point4 => base.Point4;
}