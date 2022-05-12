using FluentValidation;
using VCRC.Abstract;
using VCRC.Components;
using VCRC.Fluids;

namespace VCRC.Subcritical;

/// <summary>
///     Simple single-stage VCRC.
/// </summary>
public class SimpleVCRC : AbstractSimpleVCRC
{
    /// <summary>
    ///     Simple single-stage VCRC.
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
    public SimpleVCRC(Evaporator evaporator, Compressor compressor, Condenser condenser) :
        base(evaporator, compressor, condenser) =>
        Condenser = condenser;

    /// <summary>
    ///     Condenser as a VCRC component.
    /// </summary>
    public Condenser Condenser { get; }

    /// <summary>
    ///     Point 2 – compression stage discharge / condenser inlet.
    /// </summary>
    public new Refrigerant Point2 => base.Point2;

    /// <summary>
    ///     Point 3 – condenser outlet / EV inlet.
    /// </summary>
    public new Refrigerant Point3 => base.Point3;
}