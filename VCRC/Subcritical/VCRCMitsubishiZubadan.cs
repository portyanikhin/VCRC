using System;
using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;

namespace VCRC.Subcritical;

/// <summary>
///     Mitsubishi Zubadan VCRC.
/// </summary>
/// <remarks>
///     Two-stage VCRC with economizer, recuperator and two-phase injection to the compressor.
/// </remarks>
public class VCRCMitsubishiZubadan : AbstractVCRCMitsubishiZubadan
{
    /// <summary>
    ///     Mitsubishi Zubadan VCRC.
    /// </summary>
    /// <remarks>
    ///     Two-stage VCRC with economizer, recuperator and two-phase injection to the compressor.
    /// </remarks>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="recuperator">Recuperator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="condenser">Condenser.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ArgumentException">
    ///     Solution not found!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Superheat in the recuperator should be greater than zero!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     There should be a two-phase refrigerant at the recuperator 'hot' inlet!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at recuperator 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at recuperator 'cold' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be greater than evaporating pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be less than condensing pressure!
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
    public VCRCMitsubishiZubadan(Evaporator evaporator, Recuperator recuperator, Compressor compressor,
        Condenser condenser, EconomizerTPI economizer) :
        base(evaporator, recuperator, compressor, condenser, economizer) =>
        Condenser = condenser;

    /// <summary>
    ///     Condenser as a VCRC component.
    /// </summary>
    public Condenser Condenser { get; }
}