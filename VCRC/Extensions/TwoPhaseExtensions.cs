using EnumsNET;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using VCRC.Fluids;
using VCRC.Fluids.Attributes;

namespace VCRC.Extensions;

public static class TwoPhaseExtensions
{
    /// <summary>
    ///     Gets access to the vapor quality of the two-phase point.
    /// </summary>
    /// <param name="point">Two-phase point.</param>
    /// <returns>Vapor quality of the two-phase point.</returns>
    public static Ratio VaporQuality(this TwoPhase point) =>
        point.GetAttributes()!.Get<VaporQuality>()!.Value.Percent();
}