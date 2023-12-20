namespace VCRC;

/// <summary>
///     VCRC, which includes an economizer.
/// </summary>
public interface IHaveEconomizer
{
    /// <summary>
    ///     Economizer.
    /// </summary>
    IEconomizer Economizer { get; }
}
