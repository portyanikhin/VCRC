using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using VCRC.Components;

namespace VCRC.Abstract;

/// <summary>
///     Two-stage VCRC base class.
/// </summary>
public abstract class AbstractTwoStageVCRC : AbstractVCRC
{
    /// <summary>
    ///     Two-stage VCRC base class.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatEmitter">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    protected AbstractTwoStageVCRC(Evaporator evaporator, Compressor compressor, IHeatEmitter heatEmitter) :
        base(evaporator, compressor, heatEmitter)
    {
    }

    /// <summary>
    ///     Specific ratio of the mass flow rate of the first compression stage.
    /// </summary>
    public Ratio FirstStageSpecificMassFlow { get; } = 100.Percent();

    /// <summary>
    ///     Specific ratio of the mass flow rate of the second compression stage.
    /// </summary>
    public Ratio SecondStageSpecificMassFlow { get; protected set; }
}