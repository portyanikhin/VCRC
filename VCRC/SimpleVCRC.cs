using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.Units;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC;

/// <summary>
///     Simple single-stage VCRC.
/// </summary>
public class SimpleVCRC : SubcriticalVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Simple single-stage VCRC.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="condenser">Condenser.</param>
    /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    public SimpleVCRC(Evaporator evaporator, Compressor compressor, Condenser condenser) :
        base(evaporator, compressor, condenser)
    {
        Point2s = Refrigerant.WithState(Input.Pressure(Condenser.Pressure), Input.Entropy(Point1.Entropy));
        IsentropicSpecificWork = Point2s.Enthalpy - Point1.Enthalpy;
        SpecificWork = IsentropicSpecificWork / Compressor.IsentropicEfficiency.DecimalFractions;
        Point2 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Enthalpy(Point1.Enthalpy + SpecificWork));
        Point3 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Quality(TwoPhase.Dew.VaporQuality()));
        Point4 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Quality(TwoPhase.Bubble.VaporQuality()));
        Point5 = Condenser.Subcooling == TemperatureDelta.Zero
            ? Point4.Clone()
            : Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Temperature(Point4.Temperature - Condenser.Subcooling));
        Point6 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure), Input.Enthalpy(Point5.Enthalpy));
        SpecificCoolingCapacity = Point1.Enthalpy - Point6.Enthalpy;
        SpecificHeatingCapacity = Point2.Enthalpy - Point5.Enthalpy;
    }

    /// <summary>
    ///     Point 2s – isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – compression stage discharge / condenser inlet.
    /// </summary>
    public Refrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – dew-point on the condensing isobar.
    /// </summary>
    public Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4 – bubble-point on the condensing isobar.
    /// </summary>
    public Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – condenser outlet / expansion valve inlet.
    /// </summary>
    public Refrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – expansion valve outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point6 { get; }

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
    {
        var (coldSource, hotSource) =
            IEntropyAnalysable.SourceTemperatures(indoor, outdoor, Point1.Temperature, Point5.Temperature);
        var minSpecificWork = SpecificCoolingCapacity * (hotSource - coldSource).Kelvins / coldSource.Kelvins;
        var thermodynamicEfficiency = Ratio
            .FromDecimalFractions(minSpecificWork / SpecificWork).ToUnit(RatioUnit.Percent);
        var condenserEnergyLoss =
            Point2s.Enthalpy - Point5.Enthalpy -
            (hotSource.Kelvins * (Point2s.Entropy - Point5.Entropy).JoulesPerKilogramKelvin).JoulesPerKilogram();
        var expansionValvesEnergyLoss =
            (hotSource.Kelvins * (Point6.Entropy - Point5.Entropy).JoulesPerKilogramKelvin).JoulesPerKilogram();
        var evaporatorEnergyLoss =
            (hotSource.Kelvins * ((Point1.Entropy - Point6.Entropy).JoulesPerKilogramKelvin -
                                  (Point1.Enthalpy - Point6.Enthalpy).JoulesPerKilogram / coldSource.Kelvins))
            .JoulesPerKilogram();
        var calculatedIsentropicSpecificWork =
            minSpecificWork + condenserEnergyLoss + expansionValvesEnergyLoss + evaporatorEnergyLoss;
        var compressorEnergyLoss =
            calculatedIsentropicSpecificWork * (1.0 / Compressor.IsentropicEfficiency.DecimalFractions - 1);
        var calculatedSpecificWork = calculatedIsentropicSpecificWork + compressorEnergyLoss;
        var minSpecificWorkRatio = Ratio
            .FromDecimalFractions(minSpecificWork / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var compressorEnergyLossRatio = Ratio
            .FromDecimalFractions(compressorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var condenserEnergyLossRatio = Ratio
            .FromDecimalFractions(condenserEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var expansionValvesEnergyLossRatio = Ratio
            .FromDecimalFractions(expansionValvesEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var evaporatorEnergyLossRatio = Ratio
            .FromDecimalFractions(evaporatorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var analysisRelativeError = Ratio
            .FromDecimalFractions((calculatedIsentropicSpecificWork - IsentropicSpecificWork).Abs() /
                                  IsentropicSpecificWork).ToUnit(RatioUnit.Percent);
        return new EntropyAnalysisResult(thermodynamicEfficiency, minSpecificWorkRatio, compressorEnergyLossRatio,
            condenserEnergyLossRatio, expansionValvesEnergyLossRatio, evaporatorEnergyLossRatio, Ratio.Zero,
            Ratio.Zero, Ratio.Zero, analysisRelativeError);
    }
}