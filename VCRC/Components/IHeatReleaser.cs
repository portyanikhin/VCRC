using SharpProp;
using UnitsNet;

namespace VCRC;

/// <summary>
///     Heat releaser (condenser or gas cooler).
/// </summary>
public interface IHeatReleaser
{
    /// <summary>
    ///     Selected refrigerant name.
    /// </summary>
    public FluidsList RefrigerantName { get; }

    /// <summary>
    ///     Characteristic temperature.
    /// </summary>
    public Temperature Temperature { get; }

    /// <summary>
    ///     Absolute pressure.
    /// </summary>
    public Pressure Pressure { get; }
}