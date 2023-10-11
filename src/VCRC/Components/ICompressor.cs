namespace VCRC;

/// <summary>
///     Compressor.
/// </summary>
public interface ICompressor
{
    /// <summary>
    ///     Isentropic efficiency.
    /// </summary>
    public Ratio Efficiency { get; }
}
