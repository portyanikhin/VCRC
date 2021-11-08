using FluentValidation;
using SharpProp;
using UnitsNet;
using VCRC.Extensions;
using VCRC.Validators;

namespace VCRC
{
    /// <summary>
    ///     Subcritical VCRC
    /// </summary>
    public abstract class SubcriticalVCRC : AbstractVCRC
    {
        /// <summary>
        ///     Subcritical VCRC
        /// </summary>
        /// <param name="refrigerantName">Selected refrigerant name</param>
        /// <param name="evaporatingTemperature">Evaporating temperature</param>
        /// <param name="condensingTemperature">Condensing temperature</param>
        /// <param name="superheat">Superheat in the evaporator</param>
        /// <param name="subcooling">Subcooling in the condenser</param>
        /// <param name="isentropicEfficiency">Isentropic efficiency of the compressor</param>
        /// <param name="evaporatingPressureDefinition">
        ///     Definition of the evaporating pressure (bubble-point, dew-point or middle-point)
        /// </param>
        /// <param name="condensingPressureDefinition">
        ///     Definition of the condensing pressure (bubble-point, dew-point or middle-point)
        /// </param>
        protected SubcriticalVCRC(FluidsList refrigerantName, Temperature evaporatingTemperature,
            Temperature condensingTemperature, TemperatureDelta superheat, TemperatureDelta subcooling,
            Ratio isentropicEfficiency, TwoPhase evaporatingPressureDefinition = TwoPhase.Dew,
            TwoPhase condensingPressureDefinition = TwoPhase.Bubble) : base(refrigerantName,
            evaporatingTemperature, superheat, isentropicEfficiency, evaporatingPressureDefinition)
        {
            CondensingTemperature = condensingTemperature;
            Subcooling = subcooling;
            CondensingPressureDefinition = condensingPressureDefinition;
            new SubcriticalVCRCValidator(Refrigerant).ValidateAndThrow(this);
            CondensingPressure = Refrigerant.WithState(Input.Temperature(CondensingTemperature),
                Input.Quality(CondensingPressureDefinition.VaporQuality())).Pressure;
        }

        /// <summary>
        ///     Condensing temperature (by default, °C)
        /// </summary>
        public Temperature CondensingTemperature { get; }

        /// <summary>
        ///     Subcooling in the condenser (by default, K)
        /// </summary>
        public TemperatureDelta Subcooling { get; }

        /// <summary>
        ///     Definition of the condensing pressure (bubble-point, dew-point or middle-point)
        /// </summary>
        public TwoPhase CondensingPressureDefinition { get; }
        
        /// <summary>
        ///     Absolute condensing pressure (by default, kPa)
        /// </summary>
        public Pressure CondensingPressure { get; }
    }
}