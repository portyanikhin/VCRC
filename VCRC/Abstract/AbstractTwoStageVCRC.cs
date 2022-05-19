using System;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.Units;
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

    protected abstract SpecificEnergy FirstStageIsentropicSpecificWork { get; }

    protected abstract SpecificEnergy SecondStageIsentropicSpecificWork { get; }

    protected SpecificEnergy FirstStageSpecificWork =>
        FirstStageIsentropicSpecificWork / Compressor.IsentropicEfficiency.DecimalFractions;

    protected SpecificEnergy SecondStageSpecificWork =>
        SecondStageIsentropicSpecificWork / Compressor.IsentropicEfficiency.DecimalFractions;

    /// <summary>
    ///     Intermediate pressure.
    /// </summary>
    public Pressure IntermediatePressure =>
        Math.Sqrt(Evaporator.Pressure.Pascals * HeatEmitter.Pressure.Pascals)
            .Pascals().ToUnit(PressureUnit.Kilopascal);

    /// <summary>
    ///     Specific ratio of the mass flow rate of the first compression stage.
    /// </summary>
    public Ratio FirstStageSpecificMassFlow { get; } = 100.Percent();

    /// <summary>
    ///     Specific ratio of the mass flow rate of the second compression stage.
    /// </summary>
    public abstract Ratio SecondStageSpecificMassFlow { get; }

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        FirstStageIsentropicSpecificWork + SecondStageIsentropicSpecificWork;
}