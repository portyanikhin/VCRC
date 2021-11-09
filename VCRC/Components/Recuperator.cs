using FluentValidation;
using UnitsNet;
using VCRC.Validators;

namespace VCRC.Components
{
    /// <summary>
    ///     Recuperator as VCRC component.
    /// </summary>
    public class Recuperator
    {
        /// <summary>
        ///     Recuperator as VCRC component.
        /// </summary>
        /// <param name="superheat">Superheat in the recuperator.</param>
        public Recuperator(TemperatureDelta superheat)
        {
            Superheat = superheat;
            new RecuperatorValidator().ValidateAndThrow(this);
        }

        /// <summary>
        ///     Superheat in the recuperator.
        /// </summary>
        public TemperatureDelta Superheat { get; }
    }
}