using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.Units;
using VCRC.Components;
using VCRC.Fluids;

namespace VCRC.Transcritical;

/// <summary>
///     Simple single-stage transcritical VCRC.
/// </summary>
public class SimpleTranscriticalVCRC : TranscriticalVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Simple single-stage transcritical VCRC.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="gasCooler">Gas cooler.</param>
    /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
    public SimpleTranscriticalVCRC(Evaporator evaporator, Compressor compressor, GasCooler gasCooler) :
        base(evaporator, compressor, gasCooler)
    {
        Point2s = Refrigerant.WithState(Input.Pressure(GasCooler.Pressure),
            Input.Entropy(Point1.Entropy));
        IsentropicSpecificWork = Point2s.Enthalpy - Point1.Enthalpy;
        SpecificWork = IsentropicSpecificWork / Compressor.IsentropicEfficiency.DecimalFractions;
        Point2 = Refrigerant.WithState(Input.Pressure(GasCooler.Pressure),
            Input.Enthalpy(Point1.Enthalpy + SpecificWork));
        Point3 = Refrigerant.WithState(Input.Pressure(GasCooler.Pressure),
            Input.Temperature(GasCooler.OutletTemperature));
        Point4 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Input.Enthalpy(Point3.Enthalpy));
        SpecificCoolingCapacity = Point1.Enthalpy - Point4.Enthalpy;
        SpecificHeatingCapacity = Point2.Enthalpy - Point3.Enthalpy;
    }

    /// <summary>
    ///     Point 2s – isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – compression stage discharge / gas cooler inlet.
    /// </summary>
    public Refrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – gas cooler outlet / EV inlet.
    /// </summary>
    public Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4 – EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point4 { get; }

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
    {
        var (coldSource, hotSource) =
            IEntropyAnalysable.SourceTemperatures(indoor, outdoor, Point1.Temperature, Point3.Temperature);
        var minSpecificWork = SpecificCoolingCapacity * (hotSource - coldSource).Kelvins / coldSource.Kelvins;
        var thermodynamicPerfection = Ratio
            .FromDecimalFractions(minSpecificWork / SpecificWork).ToUnit(RatioUnit.Percent);
        var gasCoolerEnergyLoss =
            Point2s.Enthalpy - Point3.Enthalpy -
            (hotSource.Kelvins * (Point2s.Entropy - Point3.Entropy).JoulesPerKilogramKelvin).JoulesPerKilogram();
        var expansionValvesEnergyLoss =
            (hotSource.Kelvins * (Point4.Entropy - Point3.Entropy).JoulesPerKilogramKelvin).JoulesPerKilogram();
        var evaporatorEnergyLoss =
            (hotSource.Kelvins *
             ((Point1.Entropy - Point4.Entropy).JoulesPerKilogramKelvin -
              (Point1.Enthalpy - Point4.Enthalpy).JoulesPerKilogram / coldSource.Kelvins))
            .JoulesPerKilogram();
        var calculatedIsentropicSpecificWork =
            minSpecificWork + gasCoolerEnergyLoss + expansionValvesEnergyLoss + evaporatorEnergyLoss;
        var compressorEnergyLoss =
            calculatedIsentropicSpecificWork * (1.0 / Compressor.IsentropicEfficiency.DecimalFractions - 1);
        var calculatedSpecificWork = calculatedIsentropicSpecificWork + compressorEnergyLoss;
        var minSpecificWorkRatio = Ratio
            .FromDecimalFractions(minSpecificWork / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var compressorEnergyLossRatio = Ratio
            .FromDecimalFractions(compressorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var gasCoolerEnergyLossRatio = Ratio
            .FromDecimalFractions(gasCoolerEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var expansionValvesEnergyLossRatio = Ratio
            .FromDecimalFractions(expansionValvesEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var evaporatorEnergyLossRatio = Ratio
            .FromDecimalFractions(evaporatorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var analysisRelativeError = Ratio
            .FromDecimalFractions((calculatedIsentropicSpecificWork - IsentropicSpecificWork).Abs() /
                                  IsentropicSpecificWork).ToUnit(RatioUnit.Percent);
        return new EntropyAnalysisResult(thermodynamicPerfection, minSpecificWorkRatio, compressorEnergyLossRatio,
            Ratio.Zero, gasCoolerEnergyLossRatio, expansionValvesEnergyLossRatio, evaporatorEnergyLossRatio, Ratio.Zero,
            Ratio.Zero, Ratio.Zero, analysisRelativeError);
    }
}