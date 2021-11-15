using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.Units;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Validators;

namespace VCRC
{
    /// <summary>
    ///     Two-stage VCRC with incomplete intercooling.
    /// </summary>
    public class VCRCWithIncompleteIntercooling : TwoStageSubcriticalVCRC
    {
        /// <summary>
        ///     Two-stage VCRC with incomplete intercooling.
        /// </summary>
        /// <param name="evaporator">Evaporator.</param>
        /// <param name="compressor">Compressor.</param>
        /// <param name="condenser">Condenser.</param>
        /// <param name="intermediateVessel">Intermediate vessel.</param>
        public VCRCWithIncompleteIntercooling(Evaporator evaporator, Compressor compressor, Condenser condenser,
            IntermediateVessel? intermediateVessel = null) : base(evaporator, compressor, condenser)
        {
            IntermediateVessel = intermediateVessel ?? new IntermediateVessel(Evaporator, Condenser);
            Point2s = Refrigerant.WithState(Input.Pressure(IntermediateVessel.Pressure), Input.Entropy(Point1.Entropy));
            var isentropicSpecificWork1 = Point2s.Enthalpy - Point1.Enthalpy;
            var specificWork1 = isentropicSpecificWork1 / Compressor.IsentropicEfficiency.DecimalFractions;
            Point2 = Refrigerant.WithState(Input.Pressure(IntermediateVessel.Pressure),
                Input.Enthalpy(Point1.Enthalpy + specificWork1));
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
            new VCRCWithIncompleteIntercoolingValidator().ValidateAndThrow(this);
            Point9 = Refrigerant.WithState(Input.Pressure(IntermediateVessel.Pressure),
                Input.Quality(TwoPhase.Dew.VaporQuality()));
            Point10 = Refrigerant.WithState(Input.Pressure(IntermediateVessel.Pressure),
                Input.Quality(TwoPhase.Bubble.VaporQuality()));
            Point11 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure), Input.Enthalpy(Point10.Enthalpy));
            SecondStageSpecificMassFlow =
                (FirstStageSpecificMassFlow / (1 - Point8.Quality!.Value.DecimalFractions)).ToUnit(RatioUnit.Percent);
            Point3 = Refrigerant.WithState(Input.Pressure(IntermediateVessel.Pressure),
                Input.Enthalpy((FirstStageSpecificMassFlow.DecimalFractions * Point2.Enthalpy +
                                (SecondStageSpecificMassFlow - FirstStageSpecificMassFlow).DecimalFractions *
                                Point9.Enthalpy) / SecondStageSpecificMassFlow.DecimalFractions));
            Point4s = Refrigerant.WithState(Input.Pressure(Condenser.Pressure), Input.Entropy(Point3.Entropy));
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
        ///     Intermediate vessel as VCRC component.
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
        ///     Point 7 – condenser outlet / first expansion valve inlet.
        /// </summary>
        public Refrigerant Point7 { get; }

        /// <summary>
        ///     Point 8 – first expansion valve outlet / intermediate vessel inlet.
        /// </summary>
        public Refrigerant Point8 { get; }

        /// <summary>
        ///     Point 9 – intermediate vessel vapor outlet / injection of cooled vapor into the compressor.
        /// </summary>
        public Refrigerant Point9 { get; }

        /// <summary>
        ///     Point 10 – intermediate vessel liquid outlet / second expansion valve inlet.
        /// </summary>
        public Refrigerant Point10 { get; }

        /// <summary>
        ///     Point 11 – second expansion valve outlet / evaporator inlet.
        /// </summary>
        public Refrigerant Point11 { get; }
    }
}