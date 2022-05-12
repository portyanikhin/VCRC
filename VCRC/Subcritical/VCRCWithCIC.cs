using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;
using VCRC.Fluids;

namespace VCRC.Subcritical;

/// <summary>
///     Two-stage VCRC with complete intercooling.
/// </summary>
public class VCRCWithCIC : AbstractVCRCWithCIC
{
    /// <summary>
    ///     Two-stage VCRC with complete intercooling.
    /// </summary>
    /// <remarks>
    ///     If an intermediate vessel is not specified, it will be constructed automatically.
    /// </remarks>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="condenser">Condenser.</param>
    /// <param name="intermediateVessel">Intermediate vessel (optional).</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Refrigerant should not have a temperature glide!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be greater than evaporating pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be less than condensing pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     There should be a two-phase refrigerant at the intermediate vessel inlet!
    /// </exception>
    public VCRCWithCIC(Evaporator evaporator, Compressor compressor, Condenser condenser,
        IntermediateVessel? intermediateVessel = null) :
        base(evaporator, compressor, condenser, intermediateVessel) =>
        Condenser = condenser;

    /// <summary>
    ///     Condenser as a VCRC component.
    /// </summary>
    public Condenser Condenser { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge / condenser inlet.
    /// </summary>
    public new Refrigerant Point4 => base.Point4;

    /// <summary>
    ///     Point 5 – condenser outlet / first EV inlet.
    /// </summary>
    public new Refrigerant Point5 => base.Point5;
}