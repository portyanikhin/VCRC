using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;
using VCRC.Fluids;

namespace VCRC.Subcritical;

/// <summary>
///     Two-stage VCRC with parallel compression.
/// </summary>
public class VCRCWithParallelCompression : AbstractVCRCWithParallelCompression
{
    /// <summary>
    ///     Two-stage VCRC with parallel compression.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="condenser">Condenser.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Refrigerant should not have a temperature glide!
    /// </exception>
    public VCRCWithParallelCompression(Evaporator evaporator, Compressor compressor, Condenser condenser) :
        base(evaporator, compressor, condenser) =>
        Condenser = condenser;

    /// <summary>
    ///     Condenser as a VCRC component.
    /// </summary>
    public Condenser Condenser { get; }

    /// <summary>
    ///     Point 5 – condenser inlet.
    /// </summary>
    public new Refrigerant Point5 => base.Point5;

    /// <summary>
    ///     Point 6 – condenser outlet / first EV inlet.
    /// </summary>
    public new Refrigerant Point6 => base.Point6;
}