using UnitsNet;

namespace VCRC.Tests;

public static class EntropyAnalysisResultExtensions
{
    public static Ratio Sum(this EntropyAnalysisResult result)
        => result.MinSpecificWorkRatio +
           result.CompressorEnergyLossRatio +
           result.CondenserEnergyLossRatio +
           result.GasCoolerEnergyLossRatio +
           result.ExpansionValvesEnergyLossRatio +
           result.EvaporatorEnergyLossRatio +
           result.RecuperatorEnergyLossRatio +
           result.EconomizerEnergyLossRatio +
           result.MixingEnergyLossRatio;
}