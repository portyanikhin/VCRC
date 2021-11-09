using FluentValidation;
using SharpProp;
using UnitsNet;
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Validators;

namespace VCRC.Components
{
    /// <summary>
    ///     Condenser as VCRC component.
    /// </summary>
    public class Condenser
    {
        /// <summary>
        ///     Condenser as VCRC component.
        /// </summary>
        /// <param name="refrigerantName">Selected refrigerant name.</param>
        /// <param name="temperature">Condensing temperature.</param>
        /// <param name="subcooling">Subcooling in the condenser.</param>
        /// <param name="pressureDefinition">
        ///     Definition of the condensing pressure (bubble-point, dew-point or middle-point).
        /// </param>
        public Condenser(FluidsList refrigerantName, Temperature temperature, TemperatureDelta subcooling,
            TwoPhase pressureDefinition = TwoPhase.Bubble)
        {
            RefrigerantName = refrigerantName;
            Refrigerant = new Refrigerant(RefrigerantName);
            Temperature = temperature;
            Subcooling = subcooling;
            PressureDefinition = pressureDefinition;
            new CondenserValidator(Refrigerant).ValidateAndThrow(this);
            Pressure = Refrigerant.WithState(Input.Temperature(Temperature),
                Input.Quality(PressureDefinition.VaporQuality())).Pressure;
        }

        private Refrigerant Refrigerant { get; }

        /// <summary>
        ///     Selected refrigerant name.
        /// </summary>
        internal FluidsList RefrigerantName { get; }

        /// <summary>
        ///     Condensing temperature.
        /// </summary>
        public Temperature Temperature { get; }

        /// <summary>
        ///     Subcooling in the condenser.
        /// </summary>
        public TemperatureDelta Subcooling { get; }

        /// <summary>
        ///     Definition of the condensing pressure (bubble-point, dew-point or middle-point).
        /// </summary>
        public TwoPhase PressureDefinition { get; }

        /// <summary>
        ///     Condensing pressure.
        /// </summary>
        public Pressure Pressure { get; }
    }
}