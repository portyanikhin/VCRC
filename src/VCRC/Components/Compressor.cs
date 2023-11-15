namespace VCRC;

/// <inheritdoc cref="ICompressor"/>
public record Compressor : ICompressor
{
    /// <inheritdoc cref="Compressor"/>
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
