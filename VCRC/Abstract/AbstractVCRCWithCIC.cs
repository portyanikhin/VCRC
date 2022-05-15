﻿using System.Diagnostics.CodeAnalysis;
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
///     Two-stage VCRC with complete intercooling base class.
/// </summary>
public abstract class AbstractVCRCWithCIC : AbstractTwoStageVCRC, IEntropyAnalysable
{
    private Ratio _barbotageSpecificMassFlow;

    /// <summary>
    ///     Two-stage VCRC with complete intercooling base class.
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
    protected AbstractVCRCWithCIC(Evaporator evaporator, Compressor compressor, IHeatEmitter heatEmitter) :
        base(evaporator, compressor, heatEmitter)
    {
        new RefrigerantWithoutGlideValidator().ValidateAndThrow(Refrigerant);
        Point2s = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Entropy(Point1.Entropy));
        var isentropicSpecificWork1 = Point2s.Enthalpy - Point1.Enthalpy;
        var specificWork1 = isentropicSpecificWork1 / Compressor.IsentropicEfficiency.DecimalFractions;
        Point2 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Enthalpy(Point1.Enthalpy + specificWork1));
        Point3 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Quality(TwoPhase.Dew.VaporQuality()));
        Point4s = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Entropy(Point3.Entropy));
        Point5 = HeatEmitter is Condenser condenser
            ? condenser.Subcooling == TemperatureDelta.Zero
                ? Refrigerant.WithState(Input.Pressure(condenser.Pressure),
                    Input.Quality(TwoPhase.Bubble.VaporQuality()))
                : Refrigerant.WithState(Input.Pressure(condenser.Pressure),
                    Input.Temperature(condenser.Temperature - condenser.Subcooling))
            : Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
                Input.Temperature(HeatEmitter.Temperature));
        Point6 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Enthalpy(Point5.Enthalpy));
        Point7 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Quality(TwoPhase.Bubble.VaporQuality()));
        Point8 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Input.Enthalpy(Point7.Enthalpy));
        _barbotageSpecificMassFlow =
            FirstStageSpecificMassFlow *
            ((Point2.Enthalpy - Point3.Enthalpy) / (Point3.Enthalpy - Point7.Enthalpy));
        SecondStageSpecificMassFlow =
            ((FirstStageSpecificMassFlow + _barbotageSpecificMassFlow) /
             (1 - Point6.Quality!.Value.DecimalFractions)).ToUnit(RatioUnit.Percent);
        var isentropicSpecificWork2 =
            SecondStageSpecificMassFlow.DecimalFractions * (Point4s.Enthalpy - Point3.Enthalpy);
        var specificWork2 = isentropicSpecificWork2 / Compressor.IsentropicEfficiency.DecimalFractions;
        Point4 = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Enthalpy(Point3.Enthalpy + specificWork2 / SecondStageSpecificMassFlow.DecimalFractions));
        IsentropicSpecificWork = isentropicSpecificWork1 + isentropicSpecificWork2;
        SpecificWork = specificWork1 + specificWork2;
        SpecificCoolingCapacity = Point1.Enthalpy - Point8.Enthalpy;
        SpecificHeatingCapacity =
            SecondStageSpecificMassFlow.DecimalFractions * (Point4.Enthalpy - Point5.Enthalpy);
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
    ///     Point 4 – second compression stage discharge / condenser or gas cooler inlet.
    /// </summary>
    internal Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – condenser or gas cooler outlet / first EV inlet.
    /// </summary>
    internal Refrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – first EV outlet / intermediate vessel inlet.
    /// </summary>
    public Refrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – intermediate vessel liquid outlet / second EV inlet.
    /// </summary>
    public Refrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – second EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point8 { get; }

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
    {
        var (coldSource, hotSource) =
            IEntropyAnalysable.SourceTemperatures(indoor, outdoor, Point1.Temperature, Point5.Temperature);
        var minSpecificWork = SpecificCoolingCapacity * (hotSource - coldSource).Kelvins / coldSource.Kelvins;
        var thermodynamicPerfection = Ratio
            .FromDecimalFractions(minSpecificWork / SpecificWork).ToUnit(RatioUnit.Percent);
        var heatEmitterEnergyLoss =
            SecondStageSpecificMassFlow.DecimalFractions *
            (Point4s.Enthalpy - Point5.Enthalpy - (hotSource.Kelvins * (Point4s.Entropy - Point5.Entropy)
                .JoulesPerKilogramKelvin).JoulesPerKilogram());
        var expansionValvesEnergyLoss =
            (hotSource.Kelvins *
             (SecondStageSpecificMassFlow.DecimalFractions * (Point6.Entropy - Point5.Entropy) +
              FirstStageSpecificMassFlow.DecimalFractions * (Point8.Entropy - Point7.Entropy))
             .JoulesPerKilogramKelvin).JoulesPerKilogram();
        var evaporatorEnergyLoss =
            (FirstStageSpecificMassFlow.DecimalFractions * hotSource.Kelvins *
             ((Point1.Entropy - Point8.Entropy).JoulesPerKilogramKelvin -
              (Point1.Enthalpy - Point8.Enthalpy).JoulesPerKilogram / coldSource.Kelvins)).JoulesPerKilogram();
        var mixingEnergyLoss =
            (hotSource.Kelvins *
             ((FirstStageSpecificMassFlow + _barbotageSpecificMassFlow).DecimalFractions *
              Point3.Entropy.JoulesPerKilogramKelvin -
              (FirstStageSpecificMassFlow.DecimalFractions * Point2.Entropy +
               _barbotageSpecificMassFlow.DecimalFractions * Point7.Entropy)
              .JoulesPerKilogramKelvin)).JoulesPerKilogram();
        var calculatedIsentropicSpecificWork =
            minSpecificWork + heatEmitterEnergyLoss + expansionValvesEnergyLoss + evaporatorEnergyLoss +
            mixingEnergyLoss;
        var compressorEnergyLoss =
            calculatedIsentropicSpecificWork * (1.0 / Compressor.IsentropicEfficiency.DecimalFractions - 1);
        var calculatedSpecificWork = calculatedIsentropicSpecificWork + compressorEnergyLoss;
        var minSpecificWorkRatio = Ratio
            .FromDecimalFractions(minSpecificWork / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var compressorEnergyLossRatio = Ratio
            .FromDecimalFractions(compressorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var heatEmitterEnergyLossRatio = Ratio
            .FromDecimalFractions(heatEmitterEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var expansionValvesEnergyLossRatio = Ratio
            .FromDecimalFractions(expansionValvesEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var evaporatorEnergyLossRatio = Ratio
            .FromDecimalFractions(evaporatorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var mixingEnergyLossRatio = Ratio
            .FromDecimalFractions(mixingEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var analysisRelativeError = Ratio
            .FromDecimalFractions((calculatedIsentropicSpecificWork - IsentropicSpecificWork).Abs() /
                                  IsentropicSpecificWork).ToUnit(RatioUnit.Percent);
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