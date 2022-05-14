using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;
using VCRC.Fluids;

namespace VCRC.Transcritical;

/// <summary>
///     Two-stage transcritical VCRC with economizer and two-phase injection to the compressor.
/// </summary>
public class TranscriticalVCRCWithEconomizerTPI : AbstractVCRCWithEconomizerTPI
{
    /// <summary>
    ///     Two-stage transcritical VCRC with economizer and two-phase injection to the compressor.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="gasCooler">Gas cooler.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be greater than evaporating pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be less than gas cooler pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     There should be a two-phase refrigerant at the compressor injection circuit!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at economizer 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at economizer 'cold' side!
    /// </exception>
    public TranscriticalVCRCWithEconomizerTPI(Evaporator evaporator, Compressor compressor, GasCooler gasCooler,
        EconomizerTPI economizer) : base(evaporator, compressor, gasCooler, economizer) =>
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
    ///     Point 5 – gas cooler outlet / first EV inlet / economizer "hot" inlet.
    /// </summary>
    public new Refrigerant Point5 => base.Point5;
}