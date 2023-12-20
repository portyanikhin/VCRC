namespace VCRC;

/// <summary>
///     Condenser.
/// </summary>
public interface ICondenser : IHeatReleaser
{
    /// <summary>
    ///     Subcooling.
    /// </summary>
    TemperatureDelta Subcooling { get; }
}
