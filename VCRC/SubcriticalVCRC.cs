using FluentValidation;
using VCRC.Components;
using VCRC.Validators;

namespace VCRC
{
    /// <summary>
    ///     Subcritical VCRC.
    /// </summary>
    public abstract class SubcriticalVCRC : AbstractVCRC
    {
        /// <summary>
        ///     Subcritical VCRC.
        /// </summary>
        /// <param name="compressor">Compressor.</param>
        /// <param name="evaporator">Evaporator.</param>
        /// <param name="condenser">Condenser.</param>
        protected SubcriticalVCRC(Evaporator evaporator, Compressor compressor, Condenser condenser) :
            base(evaporator, compressor)
        {
            Condenser = condenser;
            new SubcriticalVCRCValidator().ValidateAndThrow(this);
        }

        /// <summary>
        ///     Condenser as VCRC component.
        /// </summary>
        public Condenser Condenser { get; }
    }
}