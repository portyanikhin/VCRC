namespace VCRC;

/// <inheritdoc cref="IEntropyAnalysisResult"/>
/// <param name="ThermodynamicPerfection">
///     Degree of thermodynamic perfection of the cycle (by default, %).
/// </param>
/// <param name="MinSpecificWorkRatio">
///     Minimum specific work
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="CompressorEnergyLossRatio">
///     Energy losses in the compressor
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="CondenserEnergyLossRatio">
///     Minimum required specific work
///     to compensate for entropy production in the condenser
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="GasCoolerEnergyLossRatio">
///     Minimum required specific work
///     to compensate for entropy production in the gas cooler
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="ExpansionValvesEnergyLossRatio">
///     Minimum required specific work
///     to compensate for entropy production in the expansion valves
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="EjectorEnergyLossRatio">
///     Minimum required specific work
///     to compensate for entropy production in the ejector
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="EvaporatorEnergyLossRatio">
///     Minimum required specific work
///     to compensate for entropy production in the evaporator
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="RecuperatorEnergyLossRatio">
///     Minimum required specific work
///     to compensate for entropy production in the recuperator
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="EconomizerEnergyLossRatio">
///     Minimum required specific work
///     to compensate for entropy production in the economizer
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="MixingEnergyLossRatio">
///     Minimum required specific work
///     to compensate for entropy production during the mixing of flows
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="AnalysisRelativeError">
///     Entropy analysis relative error (by default, %).
/// </param>
public record EntropyAnalysisResult(
    Ratio ThermodynamicPerfection,
    Ratio MinSpecificWorkRatio,
    Ratio CompressorEnergyLossRatio,
    Ratio CondenserEnergyLossRatio,
    Ratio GasCoolerEnergyLossRatio,
    Ratio ExpansionValvesEnergyLossRatio,
    Ratio EjectorEnergyLossRatio,
    Ratio EvaporatorEnergyLossRatio,
    Ratio RecuperatorEnergyLossRatio,
    Ratio EconomizerEnergyLossRatio,
    Ratio MixingEnergyLossRatio,
    Ratio AnalysisRelativeError
) : IEntropyAnalysisResult
{
    public Ratio ThermodynamicPerfection { get; } = ThermodynamicPerfection;
    public Ratio MinSpecificWorkRatio { get; } = MinSpecificWorkRatio;
    public Ratio CompressorEnergyLossRatio { get; } = CompressorEnergyLossRatio;
    public Ratio CondenserEnergyLossRatio { get; } = CondenserEnergyLossRatio;
    public Ratio GasCoolerEnergyLossRatio { get; } = GasCoolerEnergyLossRatio;

    public Ratio ExpansionValvesEnergyLossRatio { get; } =
        ExpansionValvesEnergyLossRatio;

    public Ratio EjectorEnergyLossRatio { get; } = EjectorEnergyLossRatio;
    public Ratio EvaporatorEnergyLossRatio { get; } = EvaporatorEnergyLossRatio;

    public Ratio RecuperatorEnergyLossRatio { get; } =
        RecuperatorEnergyLossRatio;

    public Ratio EconomizerEnergyLossRatio { get; } = EconomizerEnergyLossRatio;
    public Ratio MixingEnergyLossRatio { get; } = MixingEnergyLossRatio;
    public Ratio AnalysisRelativeError { get; } = AnalysisRelativeError;
}
