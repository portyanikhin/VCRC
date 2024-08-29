namespace VCRC;

/// <summary>
///     Entropy analysis extension methods.
/// </summary>
public static class EntropyAnalysisExtensions
{
    /// <summary>
    ///     Performs VCRC entropy analysis over a range of indoor and outdoor temperatures.
    /// </summary>
    /// <param name="cycles">Enumerable of VCRCs.</param>
    /// <param name="indoor">Enumerable of indoor temperatures.</param>
    /// <param name="outdoor">Enumerable of outdoor temperatures.</param>
    /// <returns>Result of the VCRC entropy analysis in range of temperatures.</returns>
    /// <exception cref="ArgumentException">Inputs should have the same length!</exception>
    public static IEntropyAnalysisResult EntropyAnalysis(
        this IEnumerable<IEntropyAnalysable> cycles,
        IEnumerable<Temperature> indoor,
        IEnumerable<Temperature> outdoor
    )
    {
        var cyclesList = cycles.ToList();
        var indoorList = indoor.ToList();
        var outdoorList = outdoor.ToList();
        return cyclesList.Count == indoorList.Count && indoorList.Count == outdoorList.Count
            ? cyclesList
                .Select((cycle, i) => cycle.EntropyAnalysis(indoorList[i], outdoorList[i]))
                .Average()
            : throw new ArgumentException("Inputs should have the same length!");
    }

    /// <summary>
    ///     Computes the average of the entropy analysis results.
    /// </summary>
    /// <param name="results">Enumerable of the entropy analysis results.</param>
    /// <returns>The average.</returns>
    public static IEntropyAnalysisResult Average(this IEnumerable<IEntropyAnalysisResult> results)
    {
        var resultsList = results.ToList();
        return new EntropyAnalysisResult(
            resultsList.Select(i => i.ThermodynamicPerfection.Percent).Average().Percent(),
            resultsList.Select(i => i.MinSpecificWorkRatio.Percent).Average().Percent(),
            resultsList.Select(i => i.CompressorEnergyLossRatio.Percent).Average().Percent(),
            resultsList.Select(i => i.CondenserEnergyLossRatio.Percent).Average().Percent(),
            resultsList.Select(i => i.GasCoolerEnergyLossRatio.Percent).Average().Percent(),
            resultsList.Select(i => i.ExpansionValvesEnergyLossRatio.Percent).Average().Percent(),
            resultsList.Select(i => i.EjectorEnergyLossRatio.Percent).Average().Percent(),
            resultsList.Select(i => i.EvaporatorEnergyLossRatio.Percent).Average().Percent(),
            resultsList.Select(i => i.RecuperatorEnergyLossRatio.Percent).Average().Percent(),
            resultsList.Select(i => i.EconomizerEnergyLossRatio.Percent).Average().Percent(),
            resultsList.Select(i => i.MixingEnergyLossRatio.Percent).Average().Percent(),
            resultsList.Select(i => i.AnalysisRelativeError.Percent).Average().Percent()
        );
    }
}
