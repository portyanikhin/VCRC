// using System;
// using System.Diagnostics.CodeAnalysis;
// using FluentValidation;
// using MathNet.Numerics;
// using MathNet.Numerics.RootFinding;
// using SharpProp;
// using UnitsNet;
// using UnitsNet.NumberExtensions.NumberToPressure;
// using UnitsNet.NumberExtensions.NumberToRatio;
// using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
// using UnitsNet.Units;
// using VCRC.Abstract.Validators;
// using VCRC.Components;
// using VCRC.Extensions;
// using VCRC.Fluids;
//
// namespace VCRC.Abstract;
//
// /// <summary>
// ///     Mitsubishi Zubadan VCRC base class.
// /// </summary>
// /// <remarks>
// ///     Two-stage VCRC with economizer, recuperator and two-phase injection to the compressor.
// /// </remarks>
// public abstract class AbstractVCRCMitsubishiZubadan : AbstractTwoStageVCRC, IEntropyAnalysable
// {
//     /// <summary>
//     ///     Mitsubishi Zubadan VCRC base class.
//     /// </summary>
//     /// <remarks>
//     ///     Two-stage VCRC with economizer, recuperator and two-phase injection to the compressor.
//     /// </remarks>
//     /// <param name="evaporator">Evaporator.</param>
//     /// <param name="recuperator">Recuperator.</param>
//     /// <param name="compressor">Compressor.</param>
//     /// <param name="heatEmitter">Condenser or gas cooler.</param>
//     /// <param name="economizer">Economizer.</param>
//     /// <exception cref="ArgumentException">
//     ///     Solution not found!
//     /// </exception>
//     /// <exception cref="ValidationException">
//     ///     Only one refrigerant should be selected!
//     /// </exception>
//     /// <exception cref="ValidationException">
//     ///     Condensing temperature should be greater than evaporating temperature!
//     /// </exception>
//     /// <exception cref="ValidationException">
//     ///     Superheat in the recuperator should be greater than zero!
//     /// </exception>
//     /// <exception cref="ValidationException">
//     ///     There should be a two-phase refrigerant at the recuperator 'hot' inlet!
//     /// </exception>
//     /// <exception cref="ValidationException">
//     ///     Wrong temperature difference at recuperator 'hot' side!
//     /// </exception>
//     /// <exception cref="ValidationException">
//     ///     Wrong temperature difference at recuperator 'cold' side!
//     /// </exception>
//     /// <exception cref="ValidationException">
//     ///     There should be a two-phase refrigerant at the compressor injection circuit!
//     /// </exception>
//     /// <exception cref="ValidationException">
//     ///     Wrong temperature difference at economizer 'hot' side!
//     /// </exception>
//     /// <exception cref="ValidationException">
//     ///     Too high temperature difference at economizer 'cold' side!
//     /// </exception>
//     protected AbstractVCRCMitsubishiZubadan(Evaporator evaporator, Recuperator recuperator, Compressor compressor,
//         IHeatEmitter heatEmitter, EconomizerTPI economizer) : base(evaporator, compressor, heatEmitter)
//     {
//         Recuperator = recuperator;
//         Economizer = economizer;
//         Point2 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
//             Input.Temperature(Point1.Temperature + Recuperator.Superheat));
//         Point3s = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
//             Input.Entropy(Point2.Entropy));
//         var isentropicSpecificWork1 = Point3s.Enthalpy - Point2.Enthalpy;
//         var specificWork1 = isentropicSpecificWork1 / Compressor.IsentropicEfficiency.DecimalFractions;
//         Point3 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
//             Input.Enthalpy(Point2.Enthalpy + specificWork1));
//         Point4 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
//             Input.Quality(TwoPhase.Dew.VaporQuality()));
//         Point5s = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
//             Input.Entropy(Point4.Entropy));
//         Point6 = HeatEmitter is Condenser condenser
//             ? condenser.Subcooling == TemperatureDelta.Zero
//                 ? Refrigerant.WithState(Input.Pressure(condenser.Pressure),
//                     Input.Quality(TwoPhase.Bubble.VaporQuality()))
//                 : Refrigerant.WithState(Input.Pressure(condenser.Pressure),
//                     Input.Temperature(condenser.Temperature - condenser.Subcooling))
//             : Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
//                 Input.Temperature(HeatEmitter.Temperature));
//         CalculateInjectionQuality(); // Also calculates Point7, Point8, Point9, Point10, Point11
//         new AbstractVCRCMitsubishiZubadanValidator().ValidateAndThrow(this);
//         Point12 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
//             Input.Enthalpy(Point11!.Enthalpy));
//         var isentropicSpecificWork2 =
//             SecondStageSpecificMassFlow.DecimalFractions * (Point5s.Enthalpy - Point4.Enthalpy);
//         var specificWork2 = isentropicSpecificWork2 / Compressor.IsentropicEfficiency.DecimalFractions;
//         Point5 = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
//             Input.Enthalpy(Point4.Enthalpy + specificWork2 / SecondStageSpecificMassFlow.DecimalFractions));
//         IsentropicSpecificWork = isentropicSpecificWork1 + isentropicSpecificWork2;
//         SpecificWork = specificWork1 + specificWork2;
//         SpecificCoolingCapacity = Point1.Enthalpy - Point12.Enthalpy;
//         SpecificHeatingCapacity =
//             SecondStageSpecificMassFlow.DecimalFractions * (Point5.Enthalpy - Point6.Enthalpy);
//     }
//
//     /// <summary>
//     ///     Recuperator as a VCRC component.
//     /// </summary>
//     public Recuperator Recuperator { get; }
//
//     /// <summary>
//     ///     Economizer as a VCRC component.
//     /// </summary>
//     public EconomizerTPI Economizer { get; }
//
//     /// <summary>
//     ///     Point 1 – evaporator outlet / recuperator "cold" inlet.
//     /// </summary>
//     public new Refrigerant Point1 => base.Point1;
//
//     /// <summary>
//     ///     Point 2 – recuperator "cold" outlet / first compression stage suction.
//     /// </summary>
//     public Refrigerant Point2 { get; }
//
//     /// <summary>
//     ///     Point 3s – first isentropic compression stage discharge.
//     /// </summary>
//     [SuppressMessage("ReSharper", "InconsistentNaming")]
//     public Refrigerant Point3s { get; }
//
//     /// <summary>
//     ///     Point 3 – first compression stage discharge.
//     /// </summary>
//     public Refrigerant Point3 { get; }
//
//     /// <summary>
//     ///     Point 4 – second compression stage suction.
//     /// </summary>
//     public Refrigerant Point4 { get; }
//
//     /// <summary>
//     ///     Point 5s – second isentropic compression stage discharge.
//     /// </summary>
//     [SuppressMessage("ReSharper", "InconsistentNaming")]
//     public Refrigerant Point5s { get; }
//
//     /// <summary>
//     ///     Point 5 – second compression stage discharge / condenser or gas cooler inlet.
//     /// </summary>
//     internal Refrigerant Point5 { get; }
//
//     /// <summary>
//     ///     Point 6 – condenser or gas cooler outlet / first EV inlet.
//     /// </summary>
//     internal Refrigerant Point6 { get; }
//
//     /// <summary>
//     ///     Point 7 – first EV outlet / recuperator "hot" inlet.
//     /// </summary>
//     public Refrigerant Point7 { get; private set; } = null!;
//
//     /// <summary>
//     ///     Point 8 – recuperator "hot" outlet / second EV inlet / economizer "hot" inlet.
//     /// </summary>
//     public Refrigerant Point8 { get; private set; } = null!;
//
//     /// <summary>
//     ///     Point 9 – second EV outlet / economizer "cold" inlet.
//     /// </summary>
//     public Refrigerant Point9 { get; private set; } = null!;
//
//     /// <summary>
//     ///     Point 10 – economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
//     /// </summary>
//     public Refrigerant Point10 { get; private set; } = null!;
//
//     /// <summary>
//     ///     Point 11 – economizer "hot" outlet / third EV inlet.
//     /// </summary>
//     public Refrigerant Point11 { get; private set; }
//
//     /// <summary>
//     ///     Point 12 – third EV outlet / evaporator inlet.
//     /// </summary>
//     public Refrigerant Point12 { get; }
//
//     private Pressure RecuperatorHighPressure
//     {
//         get
//         {
//             double ToSolve(double pressure) =>
//                 (Refrigerant.WithState(Input.Pressure(pressure.Pascals()),
//                      Input.Quality(TwoPhase.Bubble.VaporQuality())).Enthalpy -
//                  (Point6.Enthalpy - FirstStageSpecificMassFlow / SecondStageSpecificMassFlow *
//                      (Point2.Enthalpy - Point1.Enthalpy)))
//                 .JoulesPerKilogram;
//
//             return Bisection.FindRoot(
//                     ToSolve, IntermediatePressure.Pascals, HeatEmitter.Pressure.Pascals, 1e-2)
//                 .Pascals()
//                 .ToUnit(PressureUnit.Kilopascal);
//         }
//     }
//
//     public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
//     {
//         var (coldSource, hotSource) =
//             IEntropyAnalysable.SourceTemperatures(
//                 indoor, outdoor, Point1.Temperature, Point6.Temperature);
//         var minSpecificWork = SpecificCoolingCapacity *
//             (hotSource - coldSource).Kelvins / coldSource.Kelvins;
//         var thermodynamicPerfection = Ratio
//             .FromDecimalFractions(minSpecificWork / SpecificWork)
//             .ToUnit(RatioUnit.Percent);
//         var heatEmitterEnergyLoss =
//             SecondStageSpecificMassFlow.DecimalFractions *
//             (Point5s.Enthalpy - Point6.Enthalpy -
//              (hotSource.Kelvins * (Point5s.Entropy - Point6.Entropy).JoulesPerKilogramKelvin)
//              .JoulesPerKilogram());
//         var expansionValvesEnergyLoss =
//             (hotSource.Kelvins *
//              (SecondStageSpecificMassFlow.DecimalFractions * (Point7.Entropy - Point6.Entropy) +
//               (SecondStageSpecificMassFlow - FirstStageSpecificMassFlow).DecimalFractions *
//               (Point9.Entropy - Point8.Entropy) +
//               FirstStageSpecificMassFlow.DecimalFractions * (Point12.Entropy - Point11.Entropy))
//              .JoulesPerKilogramKelvin)
//             .JoulesPerKilogram();
//         var evaporatorEnergyLoss =
//             (FirstStageSpecificMassFlow.DecimalFractions * hotSource.Kelvins *
//              ((Point1.Entropy - Point12.Entropy).JoulesPerKilogramKelvin -
//               (Point1.Enthalpy - Point12.Enthalpy).JoulesPerKilogram / coldSource.Kelvins))
//             .JoulesPerKilogram();
//         var recuperatorEnergyLoss =
//             (hotSource.Kelvins *
//              (FirstStageSpecificMassFlow.DecimalFractions * (Point2.Entropy - Point1.Entropy) -
//               SecondStageSpecificMassFlow.DecimalFractions * (Point7.Entropy - Point8.Entropy))
//              .JoulesPerKilogramKelvin)
//             .JoulesPerKilogram();
//         var economizerEnergyLoss =
//             (hotSource.Kelvins *
//              ((SecondStageSpecificMassFlow - FirstStageSpecificMassFlow).DecimalFractions *
//               (Point10.Entropy - Point9.Entropy) -
//               FirstStageSpecificMassFlow.DecimalFractions *
//               (Point8.Entropy - Point11.Entropy))
//              .JoulesPerKilogramKelvin)
//             .JoulesPerKilogram();
//         var mixingEnergyLoss =
//             (hotSource.Kelvins *
//              (SecondStageSpecificMassFlow.DecimalFractions * Point4.Entropy -
//               (FirstStageSpecificMassFlow.DecimalFractions * Point3.Entropy +
//                (SecondStageSpecificMassFlow - FirstStageSpecificMassFlow)
//                .DecimalFractions * Point10.Entropy)).JoulesPerKilogramKelvin)
//             .JoulesPerKilogram();
//         var calculatedIsentropicSpecificWork =
//             minSpecificWork + heatEmitterEnergyLoss +
//             expansionValvesEnergyLoss + evaporatorEnergyLoss +
//             recuperatorEnergyLoss + economizerEnergyLoss + mixingEnergyLoss;
//         var compressorEnergyLoss =
//             calculatedIsentropicSpecificWork *
//             (1.0 / Compressor.IsentropicEfficiency.DecimalFractions - 1);
//         var calculatedSpecificWork =
//             calculatedIsentropicSpecificWork + compressorEnergyLoss;
//         var minSpecificWorkRatio = Ratio
//             .FromDecimalFractions(minSpecificWork / calculatedSpecificWork)
//             .ToUnit(RatioUnit.Percent);
//         var compressorEnergyLossRatio = Ratio
//             .FromDecimalFractions(compressorEnergyLoss / calculatedSpecificWork)
//             .ToUnit(RatioUnit.Percent);
//         var heatEmitterEnergyLossRatio = Ratio
//             .FromDecimalFractions(heatEmitterEnergyLoss / calculatedSpecificWork)
//             .ToUnit(RatioUnit.Percent);
//         var expansionValvesEnergyLossRatio = Ratio
//             .FromDecimalFractions(expansionValvesEnergyLoss / calculatedSpecificWork)
//             .ToUnit(RatioUnit.Percent);
//         var evaporatorEnergyLossRatio = Ratio
//             .FromDecimalFractions(evaporatorEnergyLoss / calculatedSpecificWork)
//             .ToUnit(RatioUnit.Percent);
//         var recuperatorEnergyLossRatio = Ratio
//             .FromDecimalFractions(recuperatorEnergyLoss / calculatedSpecificWork)
//             .ToUnit(RatioUnit.Percent);
//         var economizerEnergyLossRatio = Ratio
//             .FromDecimalFractions(economizerEnergyLoss / calculatedSpecificWork)
//             .ToUnit(RatioUnit.Percent);
//         var mixingEnergyLossRatio = Ratio
//             .FromDecimalFractions(mixingEnergyLoss / calculatedSpecificWork)
//             .ToUnit(RatioUnit.Percent);
//         var analysisRelativeError = Ratio
//             .FromDecimalFractions(
//                 (calculatedIsentropicSpecificWork - IsentropicSpecificWork).Abs() /
//                 IsentropicSpecificWork)
//             .ToUnit(RatioUnit.Percent);
//         return new EntropyAnalysisResult(
//             thermodynamicPerfection,
//             minSpecificWorkRatio,
//             compressorEnergyLossRatio,
//             HeatEmitter is Condenser ? heatEmitterEnergyLossRatio : Ratio.Zero,
//             HeatEmitter is GasCooler ? heatEmitterEnergyLossRatio : Ratio.Zero,
//             expansionValvesEnergyLossRatio,
//             evaporatorEnergyLossRatio,
//             recuperatorEnergyLossRatio,
//             economizerEnergyLossRatio,
//             mixingEnergyLossRatio,
//             analysisRelativeError);
//     }
//
//     private void CalculateInjectionQuality()
//     {
//         double ToSolve(double injectionQuality) =>
//             (CalculateEconomizerTemperatureDifference(injectionQuality.Percent()) -
//              Economizer.TemperatureDifference).Kelvins;
//
//         try
//         {
//             NewtonRaphson.FindRootNearGuess(
//                 ToSolve, Differentiate.FirstDerivativeFunc(ToSolve), 80, 1e-9, 100 - 1e-9, 1e-3);
//         }
//         catch (Exception)
//         {
//             throw new ArgumentException("Solution not found!");
//         }
//     }
//
//     private TemperatureDelta CalculateEconomizerTemperatureDifference(Ratio injectionQuality)
//     {
//         Point10 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
//             Input.Quality(injectionQuality));
//         SecondStageSpecificMassFlow =
//             FirstStageSpecificMassFlow *
//             (1 + (Point3.Enthalpy - Point4.Enthalpy) / (Point4.Enthalpy - Point10.Enthalpy));
//         Point7 = Refrigerant.WithState(Input.Pressure(RecuperatorHighPressure),
//             Input.Enthalpy(Point6.Enthalpy));
//         Point8 = Refrigerant.WithState(Input.Pressure(RecuperatorHighPressure),
//             Input.Quality(TwoPhase.Bubble.VaporQuality()));
//         Point9 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
//             Input.Enthalpy(Point8.Enthalpy));
//         Point11 = Refrigerant.WithState(Input.Pressure(RecuperatorHighPressure),
//             Input.Enthalpy(
//                 Point8.Enthalpy - (SecondStageSpecificMassFlow - FirstStageSpecificMassFlow) /
//                 FirstStageSpecificMassFlow * (Point10.Enthalpy - Point9.Enthalpy)));
//         return Point11.Temperature - Point9.Temperature;
//     }
// }

