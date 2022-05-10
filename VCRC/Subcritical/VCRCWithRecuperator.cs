using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;

namespace VCRC.Subcritical;

/// <summary>
///     Single-stage VCRC with recuperator.
/// </summary>
public class VCRCWithRecuperator : AbstractVCRCWithRecuperator
{
    /// <summary>
    ///     Single-stage VCRC with recuperator.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="recuperator">Recuperator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="condenser">Condenser.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at recuperator 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at recuperator 'cold' side!
    /// </exception>
    public VCRCWithRecuperator(Evaporator evaporator, Recuperator recuperator, Compressor compressor,
        Condenser condenser) : base(evaporator, recuperator, compressor, condenser) =>
        Condenser = condenser;

    /// <summary>
    ///     Condenser as a VCRC component.
    /// </summary>
    public Condenser Condenser { get; }
}