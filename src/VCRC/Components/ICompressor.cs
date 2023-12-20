namespace VCRC;

/// <summary>
///     Compressor.
/// </summary>
public interface ICompressor
{
    /// <summary>
    ///     Isentropic efficiency.
    /// </summary>
    Ratio Efficiency { get; }
}
