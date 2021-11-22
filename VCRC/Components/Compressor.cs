using System;
using FluentValidation;
using UnitsNet;
using VCRC.Validators;

namespace VCRC.Components
{
    /// <summary>
    ///     Compressor as a VCRC component.
    /// </summary>
    public class Compressor : IEquatable<Compressor>
    {
        /// <summary>
        ///     Compressor as a VCRC component.
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

        public bool Equals(Compressor? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object? obj) => Equals(obj as Compressor);

        public override int GetHashCode() => IsentropicEfficiency.GetHashCode();

        public static bool operator ==(Compressor? left, Compressor? right) => Equals(left, right);

        public static bool operator !=(Compressor? left, Compressor? right) => !Equals(left, right);
    }
}