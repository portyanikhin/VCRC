namespace VCRC;

/// <summary>
///     Result of the VCRC entropy analysis.
/// </summary>
public interface IEntropyAnalysisResult
{
    /// <summary>
    ///     Degree of thermodynamic perfection of the cycle (by default, %).
    /// </summary>
    public Ratio ThermodynamicPerfection { get; }

    /// <summary>
    ///     Minimum specific work
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    public Ratio MinSpecificWorkRatio { get; }

    /// <summary>
    ///     Energy losses in the compressor
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    public Ratio CompressorEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work
    ///     to compensate for entropy production in the condenser
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    public Ratio CondenserEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work
    ///     to compensate for entropy production in the gas cooler
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    public Ratio GasCoolerEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work
    ///     to compensate for entropy production in the expansion valves
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    public Ratio ExpansionValvesEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work
    ///     to compensate for entropy production in the ejector
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    public Ratio EjectorEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work
    ///     to compensate for entropy production in the evaporator
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    public Ratio EvaporatorEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work
    ///     to compensate for entropy production in the recuperator
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    public Ratio RecuperatorEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work
    ///     to compensate for entropy production in the economizer
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    public Ratio EconomizerEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work
    ///     to compensate for entropy production during the mixing of flows
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    public Ratio MixingEnergyLossRatio { get; }

    /// <summary>
    ///     Entropy analysis relative error (by default, %).
    /// </summary>
    public Ratio AnalysisRelativeError { get; }
}
