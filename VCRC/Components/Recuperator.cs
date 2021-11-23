using System;
using FluentValidation;
using UnitsNet;
using VCRC.Validators;

namespace VCRC.Components
{
    /// <summary>
    ///     Recuperator as a VCRC component.
    /// </summary>
    public class Recuperator : IEquatable<Recuperator>
    {
        /// <summary>
        ///     Recuperator as a VCRC component.
        /// </summary>
        /// <param name="superheat">Superheat in the recuperator.</param>
        /// <exception cref="ValidationException">Superheat in the recuperator should be in [0;50] K!</exception>
        public Recuperator(TemperatureDelta superheat)
        {
            Superheat = superheat;
            new RecuperatorValidator().ValidateAndThrow(this);
        }

        /// <summary>
        ///     Superheat in the recuperator.
        /// </summary>
        public TemperatureDelta Superheat { get; }

        public bool Equals(Recuperator? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object? obj) => Equals(obj as Recuperator);

        public override int GetHashCode() => Superheat.GetHashCode();

        public static bool operator ==(Recuperator? left, Recuperator? right) => Equals(left, right);

        public static bool operator !=(Recuperator? left, Recuperator? right) => !Equals(left, right);
    }
}