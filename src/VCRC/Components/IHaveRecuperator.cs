namespace VCRC;

/// <summary>
///     VCRC, which includes a recuperator.
/// </summary>
public interface IHaveRecuperator
{
    /// <summary>
    ///     Recuperator.
    /// </summary>
    IAuxiliaryHeatExchanger Recuperator { get; }
}
