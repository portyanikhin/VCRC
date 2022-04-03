using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.Units;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Validators;

namespace VCRC;

/// <summary>
///     Two-stage VCRC with economizer and two-phase injection to the compressor.
/// </summary>
public class VCRCWithEconomizerTPI : TwoStageSubcriticalVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Two-stage VCRC with economizer and two-phase injection to the compressor.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="condenser">Condenser.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be greater than evaporating pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be less than condensing pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     There should be a two-phase refrigerant at the compressor injection circuit!
    /// </exception>
    /// <exception cref="ValidationException">Wrong temperature difference at economizer 'hot' side!</exception>
    /// <exception cref="ValidationException">Too high temperature difference at economizer 'cold' side!</exception>
    public VCRCWithEconomizerTPI(Evaporator evaporator, Compressor compressor, Condenser condenser,
        EconomizerTPI economizer) : base(evaporator, compressor, condenser)
    {
        Economizer = economizer;
        Point2s = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
            Input.Entropy(Point1.Entropy));
        var isentropicSpecificWork1 = Point2s.Enthalpy - Point1.Enthalpy;
        var specificWork1 = isentropicSpecificWork1 / Compressor.IsentropicEfficiency.DecimalFractions;
        Point2 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
            Input.Enthalpy(Point1.Enthalpy + specificWork1));
        Point3 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
            Input.Quality(TwoPhase.Dew.VaporQuality()));
        Point4s = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Entropy(Point3.Entropy));
        Point5 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Quality(TwoPhase.Dew.VaporQuality()));
        Point6 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Quality(TwoPhase.Bubble.VaporQuality()));
        Point7 = Condenser.Subcooling == TemperatureDelta.Zero
            ? Point6.Clone()
            : Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Temperature(Point6.Temperature - Condenser.Subcooling));
        Point8 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
            Input.Enthalpy(Point7.Enthalpy));
        Point10 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Temperature(Point8.Temperature + Economizer.TemperatureDifference));
        Point9 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
            Input.Enthalpy(
                ((Point8.Enthalpy.JoulesPerKilogram *
                  (Point2.Enthalpy.JoulesPerKilogram - Point3.Enthalpy.JoulesPerKilogram) +
                  Point3.Enthalpy.JoulesPerKilogram *
                  (Point7.Enthalpy.JoulesPerKilogram - Point10.Enthalpy.JoulesPerKilogram)) /
                 (Point2.Enthalpy.JoulesPerKilogram - Point3.Enthalpy.JoulesPerKilogram +
                     Point7.Enthalpy.JoulesPerKilogram - Point10.Enthalpy.JoulesPerKilogram))
                .JoulesPerKilogram().ToUnit(SpecificEnergyUnit.KilojoulePerKilogram)));
        new VCRCWithEconomizerTPIValidator().ValidateAndThrow(this);
        Point11 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Input.Enthalpy(Point10.Enthalpy));
        SecondStageSpecificMassFlow =
            FirstStageSpecificMassFlow *
            (1 + (Point2.Enthalpy - Point3.Enthalpy) / (Point3.Enthalpy - Point9.Enthalpy));
        var isentropicSpecificWork2 =
            SecondStageSpecificMassFlow.DecimalFractions * (Point4s.Enthalpy - Point3.Enthalpy);
        var specificWork2 = isentropicSpecificWork2 / Compressor.IsentropicEfficiency.DecimalFractions;
        Point4 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Enthalpy(Point3.Enthalpy + specificWork2 / SecondStageSpecificMassFlow.DecimalFractions));
        IsentropicSpecificWork = isentropicSpecificWork1 + isentropicSpecificWork2;
        SpecificWork = specificWork1 + specificWork2;
        SpecificCoolingCapacity = Point1.Enthalpy - Point11.Enthalpy;
        SpecificHeatingCapacity =
            SecondStageSpecificMassFlow.DecimalFractions * (Point4.Enthalpy - Point7.Enthalpy);
    }

    /// <summary>
    ///     Economizer as a VCRC component.
    /// </summary>
    public EconomizerTPI Economizer { get; }

    /// <summary>
    ///     Point 2s – first isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – first compression stage discharge.
    /// </summary>
    public Refrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – second compression stage suction.
    /// </summary>
    public Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point4s { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge / condenser inlet.
    /// </summary>
    public Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – dew-point on the condensing isobar.
    /// </summary>
    public Refrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – bubble-point on the condensing isobar.
    /// </summary>
    public Refrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – condenser outlet / first EV inlet / economizer "hot" inlet.
    /// </summary>
    public Refrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – first EV outlet / economizer "cold" inlet.
    /// </summary>
    public Refrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
    /// </summary>
    public Refrigerant Point9 { get; }

    /// <summary>
    ///     Point 10 – economizer "hot" outlet / second EV inlet.
    /// </summary>
    public Refrigerant Point10 { get; }

    /// <summary>
    ///     Point 11 – second EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point11 { get; }

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
    {
        var (coldSource, hotSource) =
            IEntropyAnalysable.SourceTemperatures(indoor, outdoor, Point1.Temperature, Point7.Temperature);
        var minSpecificWork = SpecificCoolingCapacity * (hotSource - coldSource).Kelvins / coldSource.Kelvins;
        var thermodynamicEfficiency = Ratio
            .FromDecimalFractions(minSpecificWork / SpecificWork).ToUnit(RatioUnit.Percent);
        var condenserEnergyLoss =
            SecondStageSpecificMassFlow.DecimalFractions *
            (Point4s.Enthalpy - Point7.Enthalpy - (hotSource.Kelvins * (Point4s.Entropy - Point7.Entropy)
                .JoulesPerKilogramKelvin).JoulesPerKilogram());
        var expansionValvesEnergyLoss =
            (hotSource.Kelvins *
             ((SecondStageSpecificMassFlow - FirstStageSpecificMassFlow).DecimalFractions *
              (Point8.Entropy - Point7.Entropy) +
              FirstStageSpecificMassFlow.DecimalFractions * (Point11.Entropy - Point10.Entropy))
             .JoulesPerKilogramKelvin).JoulesPerKilogram();
        var evaporatorEnergyLoss =
            (FirstStageSpecificMassFlow.DecimalFractions * hotSource.Kelvins *
             ((Point1.Entropy - Point11.Entropy).JoulesPerKilogramKelvin -
              (Point1.Enthalpy - Point11.Enthalpy).JoulesPerKilogram / coldSource.Kelvins)).JoulesPerKilogram();
        var economizerEnergyLoss =
            (hotSource.Kelvins *
             ((SecondStageSpecificMassFlow - FirstStageSpecificMassFlow).DecimalFractions *
              (Point9.Entropy - Point8.Entropy) -
              FirstStageSpecificMassFlow.DecimalFractions *
              (Point7.Entropy - Point10.Entropy))
             .JoulesPerKilogramKelvin).JoulesPerKilogram();
        var mixingEnergyLoss =
            (hotSource.Kelvins *
             (SecondStageSpecificMassFlow.DecimalFractions * Point3.Entropy -
              (FirstStageSpecificMassFlow.DecimalFractions * Point2.Entropy +
               (SecondStageSpecificMassFlow - FirstStageSpecificMassFlow)
               .DecimalFractions * Point9.Entropy)).JoulesPerKilogramKelvin)
            .JoulesPerKilogram();
        var calculatedIsentropicSpecificWork =
            minSpecificWork + condenserEnergyLoss + expansionValvesEnergyLoss + evaporatorEnergyLoss +
            economizerEnergyLoss + mixingEnergyLoss;
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
        var economizerEnergyLossRatio = Ratio
            .FromDecimalFractions(economizerEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var mixingEnergyLossRatio = Ratio
            .FromDecimalFractions(mixingEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var analysisRelativeError = Ratio
            .FromDecimalFractions((calculatedIsentropicSpecificWork - IsentropicSpecificWork).Abs() /
                                  IsentropicSpecificWork).ToUnit(RatioUnit.Percent);
        return new EntropyAnalysisResult(thermodynamicEfficiency, minSpecificWorkRatio, compressorEnergyLossRatio,
            condenserEnergyLossRatio, expansionValvesEnergyLossRatio, evaporatorEnergyLossRatio, Ratio.Zero,
            economizerEnergyLossRatio, mixingEnergyLossRatio, analysisRelativeError);
    }
}