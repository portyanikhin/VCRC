using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;
using VCRC.Extensions;
using VCRC.Validators;

namespace VCRC
{
    /// <summary>
    ///     VCRC base class
    /// </summary>
    public abstract class AbstractVCRC
    {
        /// <summary>
        ///     VCRC base class
        /// </summary>
        /// <param name="refrigerantName">Selected refrigerant name</param>
        /// <param name="evaporatingTemperature">Evaporating temperature</param>
        /// <param name="superheat">Superheat in the evaporator</param>
        /// <param name="isentropicEfficiency">Isentropic efficiency of the compressor</param>
        protected AbstractVCRC(FluidsList refrigerantName, Temperature evaporatingTemperature,
            TemperatureDelta superheat, Ratio isentropicEfficiency)
        {
            RefrigerantName = refrigerantName;
            Refrigerant = new Refrigerant(RefrigerantName);
            EvaporatingTemperature = evaporatingTemperature;
            Superheat = superheat;
            IsentropicEfficiency = isentropicEfficiency;
            new AbstractVCRCValidator(Refrigerant).ValidateAndThrow(this);
            EvaporatingPressure = Refrigerant.WithState(Input.Temperature(EvaporatingTemperature),
                Input.Quality(EvaporatingPressureDefinition.VaporQuality())).Pressure;
            Point0 = Refrigerant.WithState(Input.Pressure(EvaporatingPressure),
                Input.Quality(TwoPhase.Dew.VaporQuality()));
            Point1 = Superheat == TemperatureDelta.Zero
                ? Point0.Clone()
                : Refrigerant.WithState(Input.Pressure(EvaporatingPressure),
                    Input.Temperature(Point0.Temperature + Superheat));
        }

        /// <summary>
        ///     Selected refrigerant name
        /// </summary>
        public FluidsList RefrigerantName { get; }

        protected Refrigerant Refrigerant { get; }

        /// <summary>
        ///     Evaporating temperature (by default, °C)
        /// </summary>
        public Temperature EvaporatingTemperature { get; }

        /// <summary>
        ///     Superheat in the evaporator (by default, K)
        /// </summary>
        public TemperatureDelta Superheat { get; }

        /// <summary>
        ///     Isentropic efficiency of the compressor (by default, %)
        /// </summary>
        public Ratio IsentropicEfficiency { get; }

        /// <summary>
        ///     Definition of the evaporating pressure (bubble-point, dew-point or middle-point)
        /// </summary>
        public TwoPhase EvaporatingPressureDefinition { get; init; } = TwoPhase.Dew;

        /// <summary>
        ///     Absolute evaporating pressure (by default, kPa)
        /// </summary>
        public Pressure EvaporatingPressure { get; }

        /// <summary>
        ///     Point 0 – dew-point on the evaporating isobar
        /// </summary>
        public Refrigerant Point0 { get; }

        /// <summary>
        ///     Point 1 – evaporator outlet
        /// </summary>
        public Refrigerant Point1 { get; }
        
        /// <summary>
        ///     Specific work of isentropic compression (by default, kJ/kg)
        /// </summary>
        public SpecificEnergy IsentropicSpecificWork { get; protected init; }

        /// <summary>
        ///     Specific work of real compression (by default, kJ/kg)
        /// </summary>
        public SpecificEnergy SpecificWork { get; protected init; }

        /// <summary>
        ///     Specific cooling capacity of the cycle (by default, kJ/kg)
        /// </summary>
        public SpecificEnergy SpecificCoolingCapacity { get; protected init; }
        
        /// <summary>
        ///     Specific heating capacity of the cycle (by default, kJ/kg)
        /// </summary>
        public SpecificEnergy SpecificHeatingCapacity { get; protected init; }

        /// <summary>
        ///     Energy efficiency ratio, aka cooling coefficient (dimensionless)
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public double EER => SpecificCoolingCapacity / SpecificWork;

        /// <summary>
        ///     Coefficient of performance, aka heating coefficient (dimensionless)
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public double COP => SpecificHeatingCapacity / SpecificWork;

        // /// <summary>
        // ///     Degree of thermodynamic perfection of the cycle (by default, %)
        // /// </summary>
        // public Ratio ThermodynamicPerfection { get; protected init; }
    }
}