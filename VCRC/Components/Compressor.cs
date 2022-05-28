using FluentValidation;
using UnitsNet;
using UnitsNet.Units;

namespace VCRC;

/// <summary>
///     Compressor as a VCRC component.
/// </summary>
public record Compressor
{
    /// <summary>
    ///     Compressor as a VCRC component.
    /// </summary>
    /// <param name="efficiency">Isentropic efficiency of the compressor.</param>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the compressor should be in (0;100) %!
    /// </exception>
    public Compressor(Ratio efficiency)
    {
        Efficiency = efficiency.ToUnit(RatioUnit.Percent);
        new CompressorValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Isentropic efficiency of the compressor.
    /// </summary>
    public Ratio Efficiency { get; }
}