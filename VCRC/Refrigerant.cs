using System;
using SharpProp;
using UnitsNet;

namespace VCRC
{
    /// <summary>
    ///     VCRC working fluid
    /// </summary>
    public class Refrigerant : Fluid
    {
        /// <summary>
        ///     VCRC working fluid
        /// </summary>
        /// <param name="name">Selected refrigerant</param>
        /// <exception cref="ArgumentException">
        ///     The selected fluid is not a refrigerant (its name should start with 'R')!
        /// </exception>
        public Refrigerant(FluidsList name) : base(name)
        {
            if (!Name.ToString().StartsWith('R'))
                throw new ArgumentException(
                    "The selected fluid is not a refrigerant (its name should start with 'R')!");
        }
        
        /// <summary>
        ///     Absolute pressure at the critical point (by default, kPa)
        /// </summary>
        /// <exception cref="NullReferenceException">Invalid critical pressure!</exception>
        public new Pressure CriticalPressure =>
            base.CriticalPressure ?? throw new NullReferenceException("Invalid critical pressure!");

        /// <summary>
        ///     Temperature at the critical point (by default, °C)
        /// </summary>
        /// <exception cref="NullReferenceException">Invalid critical temperature!</exception>
        public new Temperature CriticalTemperature =>
            base.CriticalTemperature ?? throw new NullReferenceException("Invalid critical temperature!");

        /// <summary>
        ///     Absolute pressure at the triple point (by default, kPa)
        /// </summary>
        /// <exception cref="NullReferenceException">Invalid triple pressure!</exception>
        public new Pressure TriplePressure =>
            base.TriplePressure ?? throw new NullReferenceException("Invalid triple pressure!");
        
        /// <summary>
        ///     Temperature at the triple point (by default, °C)
        /// </summary>
        /// <exception cref="NullReferenceException">Invalid triple temperature!</exception>
        public new Temperature TripleTemperature =>
            base.TripleTemperature ?? throw new NullReferenceException("Invalid triple temperature!");
    }
}