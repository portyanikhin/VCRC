using System;

namespace VCRC;

/// <summary>
///     Vapor quality of the two-phase points.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class VaporQuality : Attribute
{
    /// <summary>
    ///     Vapor quality of the two-phase points.
    /// </summary>
    /// <param name="value">Vapor quality value (%).</param>
    public VaporQuality(double value) => Value = value;

    /// <summary>
    ///     Vapor quality value (%).
    /// </summary>
    public double Value { get; }
}