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

namespace VCRC
{
    /// <summary>
    ///     Single-stage VCRC with recuperator.
    /// </summary>
    public class VCRCWithRecuperator : SubcriticalVCRC, IEntropyAnalysable
    {
        /// <summary>
        ///     Single-stage VCRC with recuperator.
        /// </summary>
        /// <param name="evaporator">Evaporator.</param>
        /// <param name="recuperator">Recuperator.</param>
        /// <param name="compressor">Compressor.</param>
        /// <param name="condenser">Condenser.</param>
        /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
        /// <exception cref="ValidationException">
        ///     Condensing temperature should be greater than evaporating temperature!
        /// </exception>
        /// <exception cref="ValidationException">Wrong temperature difference at recuperator 'hot' side!</exception>
        /// <exception cref="ValidationException">Wrong temperature difference at recuperator 'cold' side!</exception>
        public VCRCWithRecuperator(Evaporator evaporator, Recuperator recuperator, Compressor compressor,
            Condenser condenser) : base(evaporator, compressor, condenser)
        {
            Recuperator = recuperator;
            Point2 = Recuperator.Superheat == TemperatureDelta.Zero
                ? Point1.Clone()
                : Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
                    Input.Temperature(Point1.Temperature + Recuperator.Superheat));
            Point3s = Refrigerant.WithState(Input.Pressure(Condenser.Pressure), Input.Entropy(Point2.Entropy));
            IsentropicSpecificWork = Point3s.Enthalpy - Point2.Enthalpy;
            SpecificWork = IsentropicSpecificWork / Compressor.IsentropicEfficiency.DecimalFractions;
            Point3 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Enthalpy(Point2.Enthalpy + SpecificWork));
            Point4 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Quality(TwoPhase.Dew.VaporQuality()));
            Point5 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Quality(TwoPhase.Bubble.VaporQuality()));
            Point6 = Condenser.Subcooling == TemperatureDelta.Zero
                ? Point5.Clone()
                : Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                    Input.Temperature(Point5.Temperature - Condenser.Subcooling));
            Point7 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Enthalpy(Point6.Enthalpy - (Point2.Enthalpy - Point1.Enthalpy)));
            Point8 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure), Input.Enthalpy(Point7.Enthalpy));
            SpecificCoolingCapacity = Point1.Enthalpy - Point8.Enthalpy;
            SpecificHeatingCapacity = Point3.Enthalpy - Point6.Enthalpy;
            new VCRCWithRecuperatorValidator().ValidateAndThrow(this);
        }

        /// <summary>
        ///     Recuperator as a VCRC component.
        /// </summary>
        public Recuperator Recuperator { get; }

        /// <summary>
        ///     Point 2 – recuperator "cold" outlet / compression stage inlet.
        /// </summary>
        public Refrigerant Point2 { get; }

        /// <summary>
        ///     Point 3s – isentropic compression stage discharge.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public Refrigerant Point3s { get; }

        /// <summary>
        ///     Point 3 – compression stage discharge / condenser inlet.
        /// </summary>
        public Refrigerant Point3 { get; }

        /// <summary>
        ///     Point 4 – dew-point on the condensing isobar.
        /// </summary>
        public Refrigerant Point4 { get; }

        /// <summary>
        ///     Point 5 – bubble-point on the condensing isobar.
        /// </summary>
        public Refrigerant Point5 { get; }

        /// <summary>
        ///     Point 6 – condenser outlet / recuperator "hot" inlet.
        /// </summary>
        public Refrigerant Point6 { get; }

        /// <summary>
        ///     Point 7 – recuperator "hot" outlet / expansion valve inlet.
        /// </summary>
        public Refrigerant Point7 { get; }

        /// <summary>
        ///     Point 8 – expansion valve outlet / evaporator inlet.
        /// </summary>
        public Refrigerant Point8 { get; }

        public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
        {
            var (coldSource, hotSource) =
                IEntropyAnalysable.SourceTemperatures(indoor, outdoor, Point1.Temperature, Point6.Temperature);
            var minSpecificWork = SpecificCoolingCapacity * (hotSource - coldSource).Kelvins / coldSource.Kelvins;
            var thermodynamicEfficiency = Ratio
                .FromDecimalFractions(minSpecificWork / SpecificWork).ToUnit(RatioUnit.Percent);
            var condenserSuperheatRegionEnergyLoss =
                Point3s.Enthalpy - Point4.Enthalpy -
                (hotSource.Kelvins * (Point3s.Entropy - Point4.Entropy).JoulesPerKilogramKelvin).JoulesPerKilogram();
            var condenserCondensingRegionEnergyLoss =
                Point4.Enthalpy - Point5.Enthalpy -
                (hotSource.Kelvins * (Point4.Entropy - Point5.Entropy).JoulesPerKilogramKelvin).JoulesPerKilogram();
            var condenserSubcoolingRegionEnergyLoss =
                Point5.Enthalpy - Point6.Enthalpy -
                (hotSource.Kelvins * (Point5.Entropy - Point6.Entropy).JoulesPerKilogramKelvin).JoulesPerKilogram();
            var condenserEnergyLoss =
                condenserSuperheatRegionEnergyLoss + condenserCondensingRegionEnergyLoss + 
                condenserSubcoolingRegionEnergyLoss;
            var recuperatorEnergyLoss =
                (hotSource.Kelvins * ((Point2.Entropy - Point1.Entropy) - (Point6.Entropy - Point7.Entropy))
                    .JoulesPerKilogramKelvin).JoulesPerKilogram();
            var expansionValvesEnergyLoss =
                (hotSource.Kelvins * (Point8.Entropy - Point7.Entropy).JoulesPerKilogramKelvin).JoulesPerKilogram();
            var evaporatorEvaporatingRegionEnergyLoss = 
                (hotSource.Kelvins * ((Point0.Entropy - Point8.Entropy).JoulesPerKilogramKelvin -
                                      (Point0.Enthalpy - Point8.Enthalpy).JoulesPerKilogram / coldSource.Kelvins))
                .JoulesPerKilogram();
            var evaporatorSuperheatRegionEnergyLoss =
                (hotSource.Kelvins * ((Point1.Entropy - Point0.Entropy).JoulesPerKilogramKelvin -
                                      (Point1.Enthalpy - Point0.Enthalpy).JoulesPerKilogram / coldSource.Kelvins))
                .JoulesPerKilogram();
            var evaporatorEnergyLoss =
                evaporatorEvaporatingRegionEnergyLoss + evaporatorSuperheatRegionEnergyLoss;
            var calculatedIsentropicSpecificWork =
                minSpecificWork + condenserEnergyLoss + expansionValvesEnergyLoss + evaporatorEnergyLoss +
                recuperatorEnergyLoss;
            var compressorEnergyLoss = 
                calculatedIsentropicSpecificWork * (1.0 / Compressor.IsentropicEfficiency.DecimalFractions - 1);
            var calculatedSpecificWork = calculatedIsentropicSpecificWork + compressorEnergyLoss;
            var minSpecificWorkRatio = Ratio
                .FromDecimalFractions(minSpecificWork / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
            var condenserEnergyLossRatio = Ratio
                .FromDecimalFractions(condenserEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
            var expansionValvesEnergyLossRatio = Ratio
                .FromDecimalFractions(expansionValvesEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
            var evaporatorEnergyLossRatio = Ratio
                .FromDecimalFractions(evaporatorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
            var compressorEnergyLossRatio = Ratio
                .FromDecimalFractions(compressorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
            var recuperatorEnergyLossRatio = Ratio
                .FromDecimalFractions(recuperatorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
            var analysisRelativeError = Ratio
                .FromDecimalFractions((calculatedIsentropicSpecificWork - IsentropicSpecificWork).Abs() /
                                      IsentropicSpecificWork).ToUnit(RatioUnit.Percent);
            return new EntropyAnalysisResult(thermodynamicEfficiency, minSpecificWorkRatio, compressorEnergyLossRatio,
                condenserEnergyLossRatio, expansionValvesEnergyLossRatio, evaporatorEnergyLossRatio, 
                recuperatorEnergyLossRatio, analysisRelativeError);
        }
    }
}