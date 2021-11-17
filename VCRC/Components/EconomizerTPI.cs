using System;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.Units;
using VCRC.Validators;

namespace VCRC.Components
{
    /// <summary>
    ///     Economizer as component of VCRC with two-phase injection to the compressor.
    /// </summary>
    public class EconomizerTPI : IEquatable<EconomizerTPI>
    {
        /// <summary>
        ///     Economizer as component of VCRC with two-phase injection to the compressor.
        /// </summary>
        /// <param name="pressure">Absolute intermediate pressure.</param>
        /// <param name="temperatureDifference">Temperature difference at economizer "cold" side.</param>
        public EconomizerTPI(Pressure pressure, TemperatureDelta temperatureDifference)
        {
            Pressure = pressure;
            TemperatureDifference = temperatureDifference;
            new EconomizerTPIValidator().ValidateAndThrow(this);
        }
        
        /// <summary>
        ///     Economizer as component of VCRC with two-phase injection.
        ///     Note: The intermediate pressure is calculated as the square root of the product
        ///     of evaporating pressure and condensing pressure.
        /// </summary>
        /// <param name="evaporator">Evaporator.</param>
        /// <param name="condenser">Condenser.</param>
        /// <param name="temperatureDifference">Temperature difference at economizer "cold" side.</param>
        public EconomizerTPI(Evaporator evaporator, Condenser condenser,
            TemperatureDelta temperatureDifference) : this(Math
            .Sqrt(evaporator.Pressure.Pascals * condenser.Pressure.Pascals).Pascals()
            .ToUnit(PressureUnit.Kilopascal), temperatureDifference)
        {
        }

        /// <summary>
        ///     Temperature difference at economizer "cold" side.
        /// </summary>
        public TemperatureDelta TemperatureDifference { get; }

        /// <summary>
        ///     Absolute intermediate pressure.
        /// </summary>
        public Pressure Pressure { get; }

        public bool Equals(EconomizerTPI? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object? obj) => Equals(obj as EconomizerTPI);

        public override int GetHashCode() => TemperatureDifference.GetHashCode();

        public static bool operator ==(EconomizerTPI? left, EconomizerTPI? right) =>
            Equals(left, right);

        public static bool operator !=(EconomizerTPI? left, EconomizerTPI? right) =>
            !Equals(left, right);
    }
}