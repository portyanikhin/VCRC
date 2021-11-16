using System;
using FluentValidation;
using UnitsNet;
using VCRC.Validators;

namespace VCRC.Components
{
    /// <summary>
    ///     Economizer as VCRC component.
    /// </summary>
    public class Economizer : EconomizerTwoPhaseInjection, IEquatable<Economizer>
    {
        /// <summary>
        ///     Economizer as VCRC component.
        /// </summary>
        /// <param name="temperatureDifference">Temperature difference at economizer "cold" side.</param>
        /// <param name="superheat">Superheat in the economizer.</param>
        public Economizer(TemperatureDelta temperatureDifference, TemperatureDelta superheat) : 
            base(temperatureDifference)
        {
            Superheat = superheat;
            new EconomizerValidator().ValidateAndThrow(this);
        }
        
        /// <summary>
        ///     Superheat in the economizer.
        /// </summary>
        public TemperatureDelta Superheat { get; }

        public bool Equals(Economizer? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object? obj) => Equals(obj as Economizer);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Superheat);

        public static bool operator ==(Economizer? left, Economizer? right) => Equals(left, right);

        public static bool operator !=(Economizer? left, Economizer? right) => !Equals(left, right);
    }
}