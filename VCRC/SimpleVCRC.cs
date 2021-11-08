using System.Diagnostics.CodeAnalysis;
using SharpProp;
using UnitsNet;
using VCRC.Extensions;

namespace VCRC
{
    public class SimpleVCRC : SubcriticalVCRC
    {
        /// <summary>
        ///     Simple single-stage VCRC.
        /// </summary>
        /// <param name="refrigerantName">Selected refrigerant name.</param>
        /// <param name="evaporatingTemperature">Evaporating temperature.</param>
        /// <param name="condensingTemperature">Condensing temperature.</param>
        /// <param name="superheat">Superheat in the evaporator.</param>
        /// <param name="subcooling">Subcooling in the condenser.</param>
        /// <param name="isentropicEfficiency">Isentropic efficiency of the compressor.</param>
        /// <param name="evaporatingPressureDefinition">
        ///     Definition of the evaporating pressure (bubble-point, dew-point or middle-point).
        /// </param>
        /// <param name="condensingPressureDefinition">
        ///     Definition of the condensing pressure (bubble-point, dew-point or middle-point).
        /// </param>
        public SimpleVCRC(FluidsList refrigerantName, Temperature evaporatingTemperature,
            Temperature condensingTemperature, TemperatureDelta superheat, TemperatureDelta subcooling,
            Ratio isentropicEfficiency, TwoPhase evaporatingPressureDefinition = TwoPhase.Dew,
            TwoPhase condensingPressureDefinition = TwoPhase.Bubble) :
            base(refrigerantName, evaporatingTemperature, condensingTemperature,
                superheat, subcooling, isentropicEfficiency, evaporatingPressureDefinition,
                condensingPressureDefinition)
        {
            Point2s = Refrigerant.WithState(Input.Pressure(CondensingPressure), Input.Entropy(Point1.Entropy));
            IsentropicSpecificWork = Point2s.Enthalpy - Point1.Enthalpy;
            SpecificWork = IsentropicSpecificWork / IsentropicEfficiency.DecimalFractions;
            Point2 = Refrigerant.WithState(Input.Pressure(CondensingPressure),
                Input.Enthalpy(Point1.Enthalpy + SpecificWork));
            Point3 = Refrigerant.WithState(Input.Pressure(CondensingPressure),
                Input.Quality(TwoPhase.Dew.VaporQuality()));
            Point4 = Refrigerant.WithState(Input.Pressure(CondensingPressure),
                Input.Quality(TwoPhase.Bubble.VaporQuality()));
            Point5 = Subcooling == TemperatureDelta.Zero
                ? Point4.Clone()
                : Refrigerant.WithState(Input.Pressure(CondensingPressure),
                    Input.Temperature(Point4.Temperature - Subcooling));
            Point6 = Refrigerant.WithState(Input.Pressure(EvaporatingPressure), Input.Enthalpy(Point5.Enthalpy));
            SpecificCoolingCapacity = Point1.Enthalpy - Point6.Enthalpy;
            SpecificHeatingCapacity = Point2.Enthalpy - Point5.Enthalpy;
        }

        /// <summary>
        ///     Point 2s – injection from the isentropic compression stage.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public Refrigerant Point2s { get; }

        /// <summary>
        ///     Point 2 – injection from the compression stage / condenser inlet.
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
    }
}