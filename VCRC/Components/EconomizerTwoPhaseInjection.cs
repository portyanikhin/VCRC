using System;
using FluentValidation;
using UnitsNet;
using VCRC.Validators;

namespace VCRC.Components
{
    /// <summary>
    ///     Economizer as component of VCRC with two-phase injection.
    /// </summary>
    public class EconomizerTwoPhaseInjection : IEquatable<EconomizerTwoPhaseInjection>
    {
        /// <summary>
        ///     Economizer as component of VCRC with two-phase injection.
        /// </summary>
        /// <param name="temperatureDifference">Temperature difference at economizer "cold" side.</param>
        public EconomizerTwoPhaseInjection(TemperatureDelta temperatureDifference)
        {
            TemperatureDifference = temperatureDifference;
            new EconomizerTwoPhaseInjectionValidator().ValidateAndThrow(this);
        }

        /// <summary>
        ///     Temperature difference at economizer "cold" side.
        /// </summary>
        public TemperatureDelta TemperatureDifference { get; }

        public bool Equals(EconomizerTwoPhaseInjection? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object? obj) => Equals(obj as EconomizerTwoPhaseInjection);

        public override int GetHashCode() => TemperatureDifference.GetHashCode();

        public static bool operator ==(EconomizerTwoPhaseInjection? left, EconomizerTwoPhaseInjection? right) =>
            Equals(left, right);

        public static bool operator !=(EconomizerTwoPhaseInjection? left, EconomizerTwoPhaseInjection? right) =>
            !Equals(left, right);
    }
}