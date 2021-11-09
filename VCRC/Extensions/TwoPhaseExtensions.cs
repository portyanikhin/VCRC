using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using VCRC.Fluids;
using VCRC.Fluids.Attributes;

namespace VCRC.Extensions
{
    public static class TwoPhaseExtensions
    {
        /// <summary>
        ///     Gets access to the vapor quality of the two-phase point.
        /// </summary>
        /// <param name="member">The <see cref="TwoPhase" /> member.</param>
        /// <returns>Vapor quality of the two-phase point.</returns>
        public static Ratio VaporQuality(this TwoPhase member) => member.GetAttribute<VaporQuality>().Value.Percent();
    }
}