namespace VCRC;

/// <summary>
///     VCRC, which includes an economizer with two-phase injection into the compressor.
/// </summary>
public interface IHaveEconomizerWithTPI
{
    /// <summary>
    ///     Economizer.
    /// </summary>
    IAuxiliaryHeatExchanger Economizer { get; }
}
