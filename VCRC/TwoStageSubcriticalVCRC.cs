using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using VCRC.Components;

namespace VCRC
{
    /// <summary>
    ///     Two-stage subcritical VCRC.
    /// </summary>
    public abstract class TwoStageSubcriticalVCRC : SubcriticalVCRC
    {
        /// <summary>
        ///     Two-stage subcritical VCRC.
        /// </summary>
        /// <param name="compressor">Compressor.</param>
        /// <param name="evaporator">Evaporator.</param>
        /// <param name="condenser">Condenser.</param>
        /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
        /// <exception cref="ValidationException">
        ///     Condensing temperature should be greater than evaporating temperature!
        /// </exception>
        protected TwoStageSubcriticalVCRC(Evaporator evaporator, Compressor compressor, Condenser condenser) :
            base(evaporator, compressor, condenser)
        {
        }

        /// <summary>
        ///     Specific ratio of the mass flow rate of the first compression stage
        /// </summary>
        public Ratio FirstStageSpecificMassFlow { get; } = 100.Percent();

        /// <summary>
        ///     Specific ratio of the mass flow rate of the second compression stage
        /// </summary>
        public Ratio SecondStageSpecificMassFlow { get; protected init; }
    }
}