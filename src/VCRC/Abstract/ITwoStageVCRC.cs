namespace VCRC;

/// <summary>
///     Two-stage vapor-compression refrigeration cycle.
/// </summary>
public interface ITwoStageVCRC : IVCRC
{
    /// <summary>
    ///     Absolute intermediate pressure.
    /// </summary>
    Pressure IntermediatePressure { get; }

    /// <summary>
    ///     Specific mass flow rate of the intermediate circuit.
    /// </summary>
    Ratio IntermediateSpecificMassFlow { get; }
}
