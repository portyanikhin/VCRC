namespace VCRC;

/// <summary>
///     Main heat exchanger (evaporator, condenser or gas cooler).
/// </summary>
public interface IMainHeatExchanger
{
    /// <summary>
    ///     Selected refrigerant name.
    /// </summary>
    FluidsList RefrigerantName { get; }

    /// <summary>
    ///     Characteristic temperature (evaporating temperature (dew point) for the evaporator;
    ///     condensing temperature (bubble point) for the condenser;
    ///     outlet temperature for the gas cooler).
    /// </summary>
    Temperature Temperature { get; }

    /// <summary>
    ///     Absolute pressure.
    /// </summary>
    Pressure Pressure { get; }

    /// <summary>
    ///     Outlet.
    /// </summary>
    IRefrigerant Outlet { get; }
}
