using SharpProp;
using UnitsNet;

namespace VCRC.Components;

/// <summary>
///     Heat emitter (condenser or gas cooler).
/// </summary>
public interface IHeatEmitter
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