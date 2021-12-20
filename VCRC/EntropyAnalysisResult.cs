using UnitsNet;

namespace VCRC;

/// <summary>
///     Result of the VCRC entropy analysis.
/// </summary>
/// <param name="ThermodynamicPerfection">Degree of thermodynamic perfection of the cycle (by default, %).</param>
/// <param name="MinSpecificWorkRatio">
///     Minimum specific work (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="CompressorEnergyLossRatio">
///     Energy losses in the compressor (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="CondenserEnergyLossRatio">
///     Minimum required specific work to compensate for entropy production in the condenser
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="ExpansionValvesEnergyLossRatio">
///     Minimum required specific work to compensate for entropy production in the expansion valves
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="EvaporatorEnergyLossRatio">
///     Minimum required specific work to compensate for entropy production in the evaporator
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="RecuperatorEnergyLossRatio">
///     Minimum required specific work to compensate for entropy production in the recuperator
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="EconomizerEnergyLossRatio">
///     Minimum required specific work to compensate for entropy production in the economizer
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="MixingEnergyLossRatio">
///     Minimum required specific work to compensate for entropy production during the mixing of flows
///     (its percentage of the total specific work) (by default, %).
/// </param>
/// <param name="AnalysisRelativeError">Entropy analysis relative error (by default, %).</param>
public record EntropyAnalysisResult(Ratio ThermodynamicPerfection, Ratio MinSpecificWorkRatio,
    Ratio CompressorEnergyLossRatio, Ratio CondenserEnergyLossRatio, Ratio ExpansionValvesEnergyLossRatio,
    Ratio EvaporatorEnergyLossRatio, Ratio RecuperatorEnergyLossRatio, Ratio EconomizerEnergyLossRatio,
    Ratio MixingEnergyLossRatio, Ratio AnalysisRelativeError);