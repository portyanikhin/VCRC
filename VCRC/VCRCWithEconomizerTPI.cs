using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.Units;

namespace VCRC;

/// <summary>
///     Two-stage VCRC with economizer and two-phase injection to the compressor.
/// </summary>
public class VCRCWithEconomizerTPI : AbstractTwoStageVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Two-stage VCRC with economizer and two-phase injection to the compressor.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatEmitter">Condenser or gas cooler.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     There should be a two-phase refrigerant at the compressor injection circuit!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at economizer 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at economizer 'cold' side!
    /// </exception>
    public VCRCWithEconomizerTPI(Evaporator evaporator, Compressor compressor, IHeatEmitter heatEmitter,
        EconomizerTPI economizer) : base(evaporator, compressor, heatEmitter)
    {
        Economizer = economizer;
        Point2s = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Entropy(Point1.Entropy));
        Point2 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Enthalpy(Point1.Enthalpy + FirstStageSpecificWork));
        Point3 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Quality(TwoPhase.Dew.VaporQuality()));
        Point4s = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Entropy(Point3.Entropy));
        Point6 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Enthalpy(Point5.Enthalpy));
        Point8 = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Temperature(Point6.Temperature + Economizer.TemperatureDifference));
        Point7 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Enthalpy(
                ((Point6.Enthalpy.JoulesPerKilogram *
                  (Point2.Enthalpy.JoulesPerKilogram - Point3.Enthalpy.JoulesPerKilogram) +
                  Point3.Enthalpy.JoulesPerKilogram *
                  (Point5.Enthalpy.JoulesPerKilogram - Point8.Enthalpy.JoulesPerKilogram)) /
                 (Point2.Enthalpy.JoulesPerKilogram - Point3.Enthalpy.JoulesPerKilogram +
                     Point5.Enthalpy.JoulesPerKilogram - Point8.Enthalpy.JoulesPerKilogram))
                .JoulesPerKilogram()
                .ToUnit(SpecificEnergyUnit.KilojoulePerKilogram)));
        new VCRCWithEconomizerTPIValidator().ValidateAndThrow(this);
        Point9 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Input.Enthalpy(Point8.Enthalpy));
        Point4 = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Enthalpy(Point3.Enthalpy + SecondStageSpecificWork /
                SecondStageSpecificMassFlow.DecimalFractions));
    }

    /// <summary>
    ///     Economizer as a VCRC component.
    /// </summary>
    public EconomizerTPI Economizer { get; }

    /// <summary>
    ///     Point 1 – evaporator outlet / first compression stage suction.
    /// </summary>
    public new Refrigerant Point1 => base.Point1;

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
    ///     Point 4 – second compression stage discharge / condenser or gas cooler inlet.
    /// </summary>
    public Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – condenser or gas cooler outlet / first EV inlet / economizer "hot" inlet.
    /// </summary>
    public Refrigerant Point5 => HeatEmitterOutlet;

    /// <summary>
    ///     Point 6 – first EV outlet / economizer "cold" inlet.
    /// </summary>
    public Refrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
    /// </summary>
    public Refrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – economizer "hot" outlet / second EV inlet.
    /// </summary>
    public Refrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – second EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point9 { get; }

    public sealed override Ratio SecondStageSpecificMassFlow =>
        FirstStageSpecificMassFlow *
        (1 + (Point2.Enthalpy - Point3.Enthalpy) / (Point3.Enthalpy - Point7.Enthalpy));

    protected sealed override SpecificEnergy FirstStageIsentropicSpecificWork =>
        Point2s.Enthalpy - Point1.Enthalpy;

    protected sealed override SpecificEnergy SecondStageIsentropicSpecificWork =>
        SecondStageSpecificMassFlow.DecimalFractions * (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point9.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        SecondStageSpecificMassFlow.DecimalFractions * (Point4.Enthalpy - Point5.Enthalpy);

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
    {
        var (coldSource, hotSource) =
            IEntropyAnalysable.SourceTemperatures(
                indoor, outdoor, Point1.Temperature, Point5.Temperature);
        var minSpecificWork = SpecificCoolingCapacity *
            (hotSource - coldSource).Kelvins / coldSource.Kelvins;
        var thermodynamicPerfection = Ratio
            .FromDecimalFractions(minSpecificWork / SpecificWork)
            .ToUnit(RatioUnit.Percent);
        var heatEmitterEnergyLoss =
            SecondStageSpecificMassFlow.DecimalFractions *
            (Point4s.Enthalpy - Point5.Enthalpy -
             (hotSource.Kelvins * (Point4s.Entropy - Point5.Entropy).JoulesPerKilogramKelvin)
             .JoulesPerKilogram());
        var expansionValvesEnergyLoss =
            (hotSource.Kelvins *
             ((SecondStageSpecificMassFlow - FirstStageSpecificMassFlow).DecimalFractions *
              (Point6.Entropy - Point5.Entropy) +
              FirstStageSpecificMassFlow.DecimalFractions * (Point9.Entropy - Point8.Entropy))
             .JoulesPerKilogramKelvin)
            .JoulesPerKilogram();
        var evaporatorEnergyLoss =
            (FirstStageSpecificMassFlow.DecimalFractions * hotSource.Kelvins *
             ((Point1.Entropy - Point9.Entropy).JoulesPerKilogramKelvin -
              (Point1.Enthalpy - Point9.Enthalpy).JoulesPerKilogram / coldSource.Kelvins))
            .JoulesPerKilogram();
        var economizerEnergyLoss =
            (hotSource.Kelvins *
             ((SecondStageSpecificMassFlow - FirstStageSpecificMassFlow).DecimalFractions *
              (Point7.Entropy - Point6.Entropy) -
              FirstStageSpecificMassFlow.DecimalFractions *
              (Point5.Entropy - Point8.Entropy))
             .JoulesPerKilogramKelvin)
            .JoulesPerKilogram();
        var mixingEnergyLoss =
            (hotSource.Kelvins *
             (SecondStageSpecificMassFlow.DecimalFractions * Point3.Entropy -
              (FirstStageSpecificMassFlow.DecimalFractions * Point2.Entropy +
               (SecondStageSpecificMassFlow - FirstStageSpecificMassFlow)
               .DecimalFractions * Point7.Entropy)).JoulesPerKilogramKelvin)
            .JoulesPerKilogram();
        var calculatedIsentropicSpecificWork =
            minSpecificWork + heatEmitterEnergyLoss +
            expansionValvesEnergyLoss + evaporatorEnergyLoss +
            economizerEnergyLoss + mixingEnergyLoss;
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
        var economizerEnergyLossRatio = Ratio
            .FromDecimalFractions(economizerEnergyLoss / calculatedSpecificWork)
            .ToUnit(RatioUnit.Percent);
        var mixingEnergyLossRatio = Ratio
            .FromDecimalFractions(mixingEnergyLoss / calculatedSpecificWork)
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
            economizerEnergyLossRatio,
            mixingEnergyLossRatio,
            analysisRelativeError);
    }
}