using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.Units;

namespace VCRC;

/// <summary>
///     Simple single-stage VCRC.
/// </summary>
public class SimpleVCRC : AbstractVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Simple single-stage VCRC.
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
    public SimpleVCRC(Evaporator evaporator, Compressor compressor, IHeatEmitter heatEmitter) :
        base(evaporator, compressor, heatEmitter)
    {
        Point2s = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Entropy(Point1.Entropy));
        Point2 = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Enthalpy(Point1.Enthalpy + SpecificWork));
        Point4 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Input.Enthalpy(Point3.Enthalpy));
    }

    /// <summary>
    ///     Point 1 – evaporator outlet / compression stage suction.
    /// </summary>
    public new Refrigerant Point1 => base.Point1;

    /// <summary>
    ///     Point 2s – isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – compression stage discharge / condenser or gas cooler inlet.
    /// </summary>
    public Refrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – condenser or gas cooler outlet / EV inlet.
    /// </summary>
    public Refrigerant Point3 => HeatEmitterOutlet;

    /// <summary>
    ///     Point 4 – EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point4 { get; }

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy - Point1.Enthalpy;

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point4.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        Point2.Enthalpy - Point3.Enthalpy;

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
    {
        var (coldSource, hotSource) =
            IEntropyAnalysable.SourceTemperatures(
                indoor, outdoor, Point1.Temperature, Point3.Temperature);
        var minSpecificWork = SpecificCoolingCapacity *
            (hotSource - coldSource).Kelvins / coldSource.Kelvins;
        var thermodynamicPerfection = Ratio
            .FromDecimalFractions(minSpecificWork / SpecificWork)
            .ToUnit(RatioUnit.Percent);
        var heatEmitterEnergyLoss =
            Point2s.Enthalpy - Point3.Enthalpy -
            (hotSource.Kelvins * (Point2s.Entropy - Point3.Entropy).JoulesPerKilogramKelvin)
            .JoulesPerKilogram();
        var expansionValvesEnergyLoss =
            (hotSource.Kelvins * (Point4.Entropy - Point3.Entropy).JoulesPerKilogramKelvin)
            .JoulesPerKilogram();
        var evaporatorEnergyLoss =
            (hotSource.Kelvins *
             ((Point1.Entropy - Point4.Entropy).JoulesPerKilogramKelvin -
              (Point1.Enthalpy - Point4.Enthalpy).JoulesPerKilogram / coldSource.Kelvins))
            .JoulesPerKilogram();
        var calculatedIsentropicSpecificWork =
            minSpecificWork + heatEmitterEnergyLoss +
            expansionValvesEnergyLoss + evaporatorEnergyLoss;
        var compressorEnergyLoss =
            calculatedIsentropicSpecificWork *
            (1.0 / Compressor.IsentropicEfficiency.DecimalFractions - 1);
        var calculatedSpecificWork =
            calculatedIsentropicSpecificWork + compressorEnergyLoss;
        var minSpecificWorkRatio = Ratio
            .FromDecimalFractions(minSpecificWork / calculatedSpecificWork)
            .ToUnit(RatioUnit.Percent);
        var compressorEnergyLossRatio = Ratio
            .FromDecimalFractions(compressorEnergyLoss / calculatedSpecificWork)
            .ToUnit(RatioUnit.Percent);
        var heatEmitterEnergyLossRatio = Ratio
            .FromDecimalFractions(heatEmitterEnergyLoss / calculatedSpecificWork)
            .ToUnit(RatioUnit.Percent);
        var expansionValvesEnergyLossRatio = Ratio
            .FromDecimalFractions(expansionValvesEnergyLoss / calculatedSpecificWork)
            .ToUnit(RatioUnit.Percent);
        var evaporatorEnergyLossRatio = Ratio
            .FromDecimalFractions(evaporatorEnergyLoss / calculatedSpecificWork)
            .ToUnit(RatioUnit.Percent);
        var analysisRelativeError = Ratio
            .FromDecimalFractions(
                (calculatedIsentropicSpecificWork - IsentropicSpecificWork).Abs() /
                IsentropicSpecificWork)
            .ToUnit(RatioUnit.Percent);
        return new EntropyAnalysisResult(
            thermodynamicPerfection,
            minSpecificWorkRatio,
            compressorEnergyLossRatio,
            HeatEmitter is Condenser ? heatEmitterEnergyLossRatio : Ratio.Zero,
            HeatEmitter is GasCooler ? heatEmitterEnergyLossRatio : Ratio.Zero,
            expansionValvesEnergyLossRatio,
            evaporatorEnergyLossRatio,
            Ratio.Zero,
            Ratio.Zero,
            Ratio.Zero,
            analysisRelativeError);
    }
}