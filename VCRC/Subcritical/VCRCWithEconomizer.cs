using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;

namespace VCRC.Subcritical;

/// <summary>
///     Two-stage VCRC with economizer.
/// </summary>
public class VCRCWithEconomizer : AbstractVCRCWithEconomizer
{
    /// <summary>
    ///     Two-stage VCRC with economizer.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="condenser">Condenser.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be greater than evaporating pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be less than condensing pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at economizer 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at economizer 'cold' side!
    /// </exception>
    public VCRCWithEconomizer(Evaporator evaporator, Compressor compressor, Condenser condenser,
        Economizer economizer) : base(evaporator, compressor, condenser, economizer) =>
        Condenser = condenser;

    /// <summary>
    ///     Condenser as a VCRC component.
    /// </summary>
    public Condenser Condenser { get; }
}