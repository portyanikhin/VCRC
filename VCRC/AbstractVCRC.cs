using FluentValidation;
using SharpProp;
using UnitsNet;
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
    }
}