using System;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.Units;

namespace VCRC;

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
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    protected AbstractTwoStageVCRC(Evaporator evaporator, Compressor compressor, IHeatReleaser heatReleaser) :
        base(evaporator, compressor, heatReleaser)
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
        CalculateIntermediatePressure(Evaporator.Pressure, HeatReleaser.Pressure);

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        FirstStageIsentropicSpecificWork + SecondStageIsentropicSpecificWork;

    protected Pressure CalculateIntermediatePressure(Pressure low, Pressure high)
    {
        var result = GeometricMean(low, high);
        return result < Refrigerant.CriticalPressure
            ? result
            : GeometricMean(low, Refrigerant.CriticalPressure);
    }

    private static Pressure GeometricMean(Pressure low, Pressure high) =>
        Math.Sqrt(low.Pascals * high.Pascals)
            .Pascals().ToUnit(PressureUnit.Kilopascal);
}