namespace VCRC;

/// <summary>
///     VCRC, which includes an ejector.
/// </summary>
public interface IHaveEjector
{
    /// <summary>
    ///     Ejector.
    /// </summary>
    public IEjector Ejector { get; }
}
