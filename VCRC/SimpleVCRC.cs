using System.Diagnostics.CodeAnalysis;
using SharpProp;
using UnitsNet;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC
{
    /// <summary>
    ///     Simple single-stage VCRC.
    /// </summary>
    public class SimpleVCRC : SubcriticalVCRC
    {
        /// <summary>
        ///     Simple single-stage VCRC.
        /// </summary>
        /// <param name="compressor">Compressor.</param>
        /// <param name="evaporator">Evaporator.</param>
        /// <param name="condenser">Condenser.</param>
        public SimpleVCRC(Evaporator evaporator, Compressor compressor, Condenser condenser) :
            base(evaporator, compressor, condenser)
        {
            Point2s = Refrigerant.WithState(Input.Pressure(Condenser.Pressure), Input.Entropy(Point1.Entropy));
            IsentropicSpecificWork = Point2s.Enthalpy - Point1.Enthalpy;
            SpecificWork = IsentropicSpecificWork / Compressor.IsentropicEfficiency.DecimalFractions;
            Point2 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Enthalpy(Point1.Enthalpy + SpecificWork));
            Point3 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Quality(TwoPhase.Dew.VaporQuality()));
            Point4 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Quality(TwoPhase.Bubble.VaporQuality()));
            Point5 = Condenser.Subcooling == TemperatureDelta.Zero
                ? Point4.Clone()
                : Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                    Input.Temperature(Point4.Temperature - Condenser.Subcooling));
            Point6 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure), Input.Enthalpy(Point5.Enthalpy));
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