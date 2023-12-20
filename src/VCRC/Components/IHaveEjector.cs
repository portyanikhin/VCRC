namespace VCRC;

/// <summary>
///     VCRC, which includes an ejector.
/// </summary>
public interface IHaveEjector
{
    /// <summary>
    ///     Ejector.
    /// </summary>
    IEjector Ejector { get; }
}
