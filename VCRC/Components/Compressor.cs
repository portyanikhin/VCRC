using FluentValidation;
using UnitsNet;
using VCRC.Validators;

namespace VCRC.Components
{
    /// <summary>
    ///     Compressor as VCRC component.
    /// </summary>
    public class Compressor
    {
        /// <summary>
        ///     Compressor as VCRC component.
        /// </summary>
        /// <param name="isentropicEfficiency">Isentropic efficiency of the compressor.</param>
        public Compressor(Ratio isentropicEfficiency)
        {
            IsentropicEfficiency = isentropicEfficiency;
            new CompressorValidator().ValidateAndThrow(this);
        }

        /// <summary>
        ///     Isentropic efficiency of the compressor.
        /// </summary>
        public Ratio IsentropicEfficiency { get; }
    }
}