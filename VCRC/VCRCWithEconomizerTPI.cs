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
    ///     Two-stage VCRC with economizer and two-phase injection to the compressor.
    /// </summary>
    public class VCRCWithEconomizerTPI : TwoStageSubcriticalVCRC
    {
        /// <summary>
        ///     Two-stage VCRC with economizer and two-phase injection to the compressor.
        /// </summary>
        /// <param name="evaporator">Evaporator.</param>
        /// <param name="compressor">Compressor.</param>
        /// <param name="condenser">Condenser.</param>
        /// <param name="economizer">Economizer.</param>
        public VCRCWithEconomizerTPI(Evaporator evaporator, Compressor compressor, Condenser condenser,
            EconomizerTPI economizer) : base(evaporator, compressor, condenser)
        {
            Economizer = economizer;
            Point2s = Refrigerant.WithState(Input.Pressure(Economizer.Pressure), Input.Entropy(Point1.Entropy));
            var isentropicSpecificWork1 = Point2s.Enthalpy - Point1.Enthalpy;
            var specificWork1 = isentropicSpecificWork1 / Compressor.IsentropicEfficiency.DecimalFractions;
            Point2 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
                Input.Enthalpy(Point1.Enthalpy + specificWork1));
            Point3 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
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
            Point8 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
                Input.Enthalpy(Point7.Enthalpy));
            Point10 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Temperature(Point8.Temperature + Economizer.TemperatureDifference));
            Point9 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
                Input.Enthalpy(((Point8.Enthalpy.JoulesPerKilogram *
                                 (Point2.Enthalpy.JoulesPerKilogram - Point3.Enthalpy.JoulesPerKilogram) +
                                 Point3.Enthalpy.JoulesPerKilogram * (Point7.Enthalpy.JoulesPerKilogram -
                                                                      Point10.Enthalpy.JoulesPerKilogram)) /
                                (Point2.Enthalpy.JoulesPerKilogram - Point3.Enthalpy.JoulesPerKilogram +
                                    Point7.Enthalpy.JoulesPerKilogram - Point10.Enthalpy.JoulesPerKilogram))
                    .JoulesPerKilogram().ToUnit(SpecificEnergyUnit.KilojoulePerKilogram)));
            new VCRCWithEconomizerTPIValidator().ValidateAndThrow(this);
            Point11 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure), Input.Enthalpy(Point10.Enthalpy));
            SecondStageSpecificMassFlow = FirstStageSpecificMassFlow *
                                          (1 + (Point2.Enthalpy - Point3.Enthalpy) /
                                              (Point3.Enthalpy - Point9.Enthalpy));
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
        ///     Point 7 – condenser outlet /
        ///     inlet to the expansion valve of the injection circuit /
        ///     inlet of the main stream into the economizer ("hot" inlet).
        /// </summary>
        public Refrigerant Point7 { get; }
        
        /// <summary>
        ///     Point 8 – outlet from the expansion valve of the injection circuit /
        ///     inlet of the injected stream into the economizer ("cold" inlet).
        /// </summary>
        public Refrigerant Point8 { get; }
        
        /// <summary>
        ///     Point 9 – outlet of the injected stream from the economizer ("cold" outlet) /
        ///     injection of two-phase refrigerant into the compressor.
        /// </summary>
        public Refrigerant Point9 { get; }

        /// <summary>
        ///     Point 10 – outlet of the main stream from the economizer ("hot" outlet) /
        ///     inlet to the expansion valve of the evaporator circuit.
        /// </summary>
        public Refrigerant Point10 { get; }

        /// <summary>
        ///     Point 11 – outlet from the expansion valve of the evaporator circuit / evaporator inlet.
        /// </summary>
        public Refrigerant Point11 { get; }
    }
}