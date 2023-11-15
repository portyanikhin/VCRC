namespace VCRC.Tests;

internal static class EntropyAnalysisResultExtensions
{
    public static Ratio Sum(this IEntropyAnalysisResult result) =>
        result.MinSpecificWorkRatio
        + result.CompressorEnergyLossRatio
        + result.CondenserEnergyLossRatio
        + result.GasCoolerEnergyLossRatio
        + result.ExpansionValvesEnergyLossRatio
        + result.EjectorEnergyLossRatio
        + result.EvaporatorEnergyLossRatio
        + result.RecuperatorEnergyLossRatio
        + result.EconomizerEnergyLossRatio
        + result.MixingEnergyLossRatio;
}
