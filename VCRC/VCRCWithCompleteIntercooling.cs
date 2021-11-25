﻿using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.Units;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Validators;

namespace VCRC
{
    /// <summary>
    ///     Two-stage VCRC with complete intercooling.
    /// </summary>
    public class VCRCWithCompleteIntercooling : TwoStageSubcriticalVCRC, IEntropyAnalysable
    {
        private Ratio _barbotageSpecificMassFlow;

        /// <summary>
        ///     Two-stage VCRC with complete intercooling.
        /// </summary>
        /// <param name="evaporator">Evaporator.</param>
        /// <param name="compressor">Compressor.</param>
        /// <param name="condenser">Condenser.</param>
        /// <param name="intermediateVessel">Intermediate vessel.</param>
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
        ///     There should be a two-phase refrigerant at the intermediate vessel inlet!
        /// </exception>
        public VCRCWithCompleteIntercooling(Evaporator evaporator, Compressor compressor, Condenser condenser,
            IntermediateVessel? intermediateVessel = null) : base(evaporator, compressor, condenser)
        {
            IntermediateVessel = intermediateVessel ?? new IntermediateVessel(Evaporator, Condenser);
            Point2s = Refrigerant.WithState(Input.Pressure(IntermediateVessel.Pressure), Input.Entropy(Point1.Entropy));
            var isentropicSpecificWork1 = Point2s.Enthalpy - Point1.Enthalpy;
            var specificWork1 = isentropicSpecificWork1 / Compressor.IsentropicEfficiency.DecimalFractions;
            Point2 = Refrigerant.WithState(Input.Pressure(IntermediateVessel.Pressure),
                Input.Enthalpy(Point1.Enthalpy + specificWork1));
            Point3 = Refrigerant.WithState(Input.Pressure(IntermediateVessel.Pressure),
                Input.Quality(TwoPhase.Dew.VaporQuality()));
            Point4s = Refrigerant.WithState(Input.Pressure(Condenser.Pressure), Input.Entropy(Point3.Entropy));
            Point5 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Quality(TwoPhase.Dew.VaporQuality()));
            Point6 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Quality(TwoPhase.Bubble.VaporQuality()));
            Point7 = Condenser.Subcooling == TemperatureDelta.Zero
                ? Point6.Clone()
                : Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                    Input.Temperature(Point6.Temperature - Condenser.Subcooling));
            Point8 = Refrigerant.WithState(Input.Pressure(IntermediateVessel.Pressure),
                Input.Enthalpy(Point7.Enthalpy));
            new VCRCWithCompleteIntercoolingValidator().ValidateAndThrow(this);
            Point9 = Refrigerant.WithState(Input.Pressure(IntermediateVessel.Pressure),
                Input.Quality(TwoPhase.Bubble.VaporQuality()));
            Point10 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure), Input.Enthalpy(Point9.Enthalpy));
            _barbotageSpecificMassFlow = FirstStageSpecificMassFlow *
                                         ((Point2.Enthalpy - Point3.Enthalpy) / (Point3.Enthalpy - Point9.Enthalpy));
            SecondStageSpecificMassFlow =
                ((FirstStageSpecificMassFlow + _barbotageSpecificMassFlow) /
                 (1 - Point8.Quality!.Value.DecimalFractions)).ToUnit(RatioUnit.Percent);
            var isentropicSpecificWork2 =
                SecondStageSpecificMassFlow.DecimalFractions * (Point4s.Enthalpy - Point3.Enthalpy);
            var specificWork2 = isentropicSpecificWork2 / Compressor.IsentropicEfficiency.DecimalFractions;
            Point4 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Enthalpy(Point3.Enthalpy + specificWork2 / SecondStageSpecificMassFlow.DecimalFractions));
            IsentropicSpecificWork = isentropicSpecificWork1 + isentropicSpecificWork2;
            SpecificWork = specificWork1 + specificWork2;
            SpecificCoolingCapacity = Point1.Enthalpy - Point10.Enthalpy;
            SpecificHeatingCapacity =
                SecondStageSpecificMassFlow.DecimalFractions * (Point4.Enthalpy - Point7.Enthalpy);
        }

        /// <summary>
        ///     Intermediate vessel as a VCRC component.
        /// </summary>
        public IntermediateVessel IntermediateVessel { get; }

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
        ///     Point 7 – condenser outlet / first expansion valve inlet.
        /// </summary>
        public Refrigerant Point7 { get; }

        /// <summary>
        ///     Point 8 – first expansion valve outlet / intermediate vessel inlet.
        /// </summary>
        public Refrigerant Point8 { get; }

        /// <summary>
        ///     Point 9 – intermediate vessel liquid outlet / second expansion valve inlet.
        /// </summary>
        public Refrigerant Point9 { get; }

        /// <summary>
        ///     Point 10 – second expansion valve outlet / evaporator inlet.
        /// </summary>
        public Refrigerant Point10 { get; }

        public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
        {
            var (coldSource, hotSource) =
                IEntropyAnalysable.SourceTemperatures(indoor, outdoor, Point1.Temperature, Point7.Temperature);
            var minSpecificWork = SpecificCoolingCapacity * (hotSource - coldSource).Kelvins / coldSource.Kelvins;
            var thermodynamicEfficiency = Ratio
                .FromDecimalFractions(minSpecificWork / SpecificWork).ToUnit(RatioUnit.Percent);
            var condenserEnergyLoss =
                SecondStageSpecificMassFlow.DecimalFractions * (Point4s.Enthalpy - Point7.Enthalpy -
                                                                (hotSource.Kelvins * (Point4s.Entropy - Point7.Entropy)
                                                                    .JoulesPerKilogramKelvin).JoulesPerKilogram());
            var expansionValvesEnergyLoss =
                (hotSource.Kelvins *
                 (SecondStageSpecificMassFlow.DecimalFractions * (Point8.Entropy - Point7.Entropy) +
                  FirstStageSpecificMassFlow.DecimalFractions * (Point10.Entropy - Point9.Entropy))
                 .JoulesPerKilogramKelvin).JoulesPerKilogram();
            var evaporatorEnergyLoss =
                (FirstStageSpecificMassFlow.DecimalFractions * hotSource.Kelvins *
                 ((Point1.Entropy - Point10.Entropy).JoulesPerKilogramKelvin -
                  (Point1.Enthalpy - Point10.Enthalpy).JoulesPerKilogram / coldSource.Kelvins)).JoulesPerKilogram();
            var mixingEnergyLoss = (hotSource.Kelvins *
                                    ((FirstStageSpecificMassFlow + _barbotageSpecificMassFlow).DecimalFractions *
                                     Point3.Entropy.JoulesPerKilogramKelvin -
                                     (FirstStageSpecificMassFlow.DecimalFractions * Point2.Entropy +
                                      _barbotageSpecificMassFlow.DecimalFractions * Point9.Entropy)
                                     .JoulesPerKilogramKelvin)).JoulesPerKilogram();
            var calculatedIsentropicSpecificWork =
                minSpecificWork + condenserEnergyLoss + expansionValvesEnergyLoss + evaporatorEnergyLoss +
                mixingEnergyLoss;
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
            var mixingEnergyLossRatio = Ratio
                .FromDecimalFractions(mixingEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
            var analysisRelativeError = Ratio
                .FromDecimalFractions((calculatedIsentropicSpecificWork - IsentropicSpecificWork).Abs() /
                                      IsentropicSpecificWork).ToUnit(RatioUnit.Percent);
            return new EntropyAnalysisResult(thermodynamicEfficiency, minSpecificWorkRatio, compressorEnergyLossRatio,
                condenserEnergyLossRatio, expansionValvesEnergyLossRatio, evaporatorEnergyLossRatio, Ratio.Zero,
                Ratio.Zero, mixingEnergyLossRatio, analysisRelativeError);
        }
    }
}