namespace VCRC;

/// <summary>
///     Result of the VCRC entropy analysis.
/// </summary>
public interface IEntropyAnalysisResult
{
    /// <summary>
    ///     Thermodynamic perfection degree of the cycle (by default, %).
    /// </summary>
    Ratio ThermodynamicPerfection { get; }

    /// <summary>
    ///     Minimum specific work (its percentage of the total specific work) (by default, %).
    /// </summary>
    Ratio MinSpecificWorkRatio { get; }

    /// <summary>
    ///     Energy losses in the compressor
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    Ratio CompressorEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work to compensate for entropy production in the condenser
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    Ratio CondenserEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work to compensate for entropy production in the gas cooler
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    Ratio GasCoolerEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work to compensate for entropy production
    ///     in the expansion valves (its percentage of the total specific work) (by default, %).
    /// </summary>
    Ratio ExpansionValvesEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work to compensate for entropy production in the ejector
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    Ratio EjectorEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work to compensate for entropy production in the evaporator
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    Ratio EvaporatorEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work to compensate for entropy production in the recuperator
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    Ratio RecuperatorEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work to compensate for entropy production in the economizer
    ///     (its percentage of the total specific work) (by default, %).
    /// </summary>
    Ratio EconomizerEnergyLossRatio { get; }

    /// <summary>
    ///     Minimum required specific work to compensate for entropy production during
    ///     the mixing of flows (its percentage of the total specific work) (by default, %).
    /// </summary>
    Ratio MixingEnergyLossRatio { get; }

    /// <summary>
    ///     Entropy analysis relative error (by default, %).
    /// </summary>
    Ratio AnalysisRelativeError { get; }
}
