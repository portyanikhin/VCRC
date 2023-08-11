namespace VCRC;

/// <summary>
///     Compressor.
/// </summary>
public record Compressor : ICompressor
{
    /// <summary>
    ///     Compressor.
    /// </summary>
    /// <param name="efficiency">Isentropic efficiency.</param>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the compressor should be in (0;100) %!
    /// </exception>
    public Compressor(Ratio efficiency)
    {
        Efficiency = efficiency.ToUnit(RatioUnit.Percent);
        new CompressorValidator().ValidateAndThrow(this);
    }

    public Ratio Efficiency { get; }
}
