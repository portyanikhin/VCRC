using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.Units;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Fluids.Validators;

namespace VCRC.Abstract;

/// <summary>
///     Two-stage VCRC with parallel compression base class.
/// </summary>
public abstract class AbstractVCRCWithParallelCompression : AbstractTwoStageVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Two-stage VCRC with parallel compression base class.
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
    /// <exception cref="ValidationException">
    ///     Refrigerant should not have a temperature glide!
    /// </exception>
    protected AbstractVCRCWithParallelCompression(Evaporator evaporator, Compressor compressor,
        IHeatEmitter heatEmitter) : base(evaporator, compressor, heatEmitter)
    {
        new RefrigerantWithoutGlideValidator().ValidateAndThrow(Refrigerant);
        Point2s = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Entropy(Point1.Entropy));
        Point2 = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Enthalpy(Point1.Enthalpy + FirstStageSpecificWork));
        Point3 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Quality(TwoPhase.Dew.VaporQuality()));
        Point7 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Enthalpy(Point6.Enthalpy));
        Point4s = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Entropy(Point3.Entropy));
        Point4 = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Enthalpy(Point3.Enthalpy + SecondStageSpecificWork /
                SecondStageSpecificMassFlow.DecimalFractions));
        Point5s = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Enthalpy(
                (FirstStageSpecificMassFlow.DecimalFractions * Point2s.Enthalpy +
                 SecondStageSpecificMassFlow.DecimalFractions * Point4s.Enthalpy) /
                (FirstStageSpecificMassFlow + SecondStageSpecificMassFlow).DecimalFractions));
        Point5 = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Enthalpy(
                (FirstStageSpecificMassFlow.DecimalFractions * Point2.Enthalpy +
                 SecondStageSpecificMassFlow.DecimalFractions * Point4.Enthalpy) /
                (FirstStageSpecificMassFlow + SecondStageSpecificMassFlow).DecimalFractions));
        Point8 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Quality(TwoPhase.Bubble.VaporQuality()));
        Point9 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Input.Enthalpy(Point8.Enthalpy));
    }

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
    ///     Point 3 – intermediate vessel vapor outlet / second compression stage suction.
    /// </summary>
    public Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point4s { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge.
    /// </summary>
    public Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5s – condenser or gas cooler inlet (for isentropic compression).
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private Refrigerant Point5s { get; }

    /// <summary>
    ///     Point 5 – condenser or gas cooler inlet.
    /// </summary>
    internal Refrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – condenser or gas cooler outlet / first EV inlet.
    /// </summary>
    internal Refrigerant Point6 => HeatEmitterOutlet;

    /// <summary>
    ///     Point 7 – first EV outlet / intermediate vessel inlet.
    /// </summary>
    public Refrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – intermediate vessel liquid outlet / second EV inlet.
    /// </summary>
    public Refrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – second EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point9 { get; }

    public sealed override Ratio SecondStageSpecificMassFlow =>
        FirstStageSpecificMassFlow *
        Point7.Quality!.Value.DecimalFractions /
        (1 - Point7.Quality!.Value.DecimalFractions);

    protected sealed override SpecificEnergy FirstStageIsentropicSpecificWork =>
        Point2s.Enthalpy - Point1.Enthalpy;

    protected sealed override SpecificEnergy SecondStageIsentropicSpecificWork =>
        SecondStageSpecificMassFlow.DecimalFractions * (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point9.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        (FirstStageSpecificMassFlow + SecondStageSpecificMassFlow).DecimalFractions *
        (Point5.Enthalpy - Point6.Enthalpy);

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
    {
        var (coldSource, hotSource) =
            IEntropyAnalysable.SourceTemperatures(
                indoor, outdoor, Point1.Temperature, Point6.Temperature);
        var minSpecificWork = SpecificCoolingCapacity *
            (hotSource - coldSource).Kelvins / coldSource.Kelvins;
        var thermodynamicPerfection = Ratio
            .FromDecimalFractions(minSpecificWork / SpecificWork)
            .ToUnit(RatioUnit.Percent);
        var heatEmitterEnergyLoss =
            (FirstStageSpecificMassFlow + SecondStageSpecificMassFlow).DecimalFractions *
            (Point5s.Enthalpy - Point6.Enthalpy -
             (hotSource.Kelvins * (Point5s.Entropy - Point6.Entropy).JoulesPerKilogramKelvin)
             .JoulesPerKilogram());
        var expansionValvesEnergyLoss =
            (hotSource.Kelvins *
             ((FirstStageSpecificMassFlow + SecondStageSpecificMassFlow).DecimalFractions *
              (Point7.Entropy - Point6.Entropy) +
              FirstStageSpecificMassFlow.DecimalFractions *
              (Point9.Entropy - Point8.Entropy))
             .JoulesPerKilogramKelvin)
            .JoulesPerKilogram();
        var evaporatorEnergyLoss =
            (FirstStageSpecificMassFlow.DecimalFractions * hotSource.Kelvins *
             ((Point1.Entropy - Point9.Entropy).JoulesPerKilogramKelvin -
              (Point1.Enthalpy - Point9.Enthalpy).JoulesPerKilogram / coldSource.Kelvins))
            .JoulesPerKilogram();
        var mixingEnergyLoss =
            (hotSource.Kelvins *
             ((FirstStageSpecificMassFlow + SecondStageSpecificMassFlow).DecimalFractions * Point5.Entropy -
              (FirstStageSpecificMassFlow.DecimalFractions * Point2.Entropy +
               SecondStageSpecificMassFlow.DecimalFractions * Point4.Entropy))
             .JoulesPerKilogramKelvin)
            .JoulesPerKilogram();
        var calculatedIsentropicSpecificWork =
            minSpecificWork + heatEmitterEnergyLoss +
            expansionValvesEnergyLoss + evaporatorEnergyLoss + mixingEnergyLoss;
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
            Ratio.Zero,
            mixingEnergyLossRatio,
            analysisRelativeError);
    }
}