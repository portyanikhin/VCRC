namespace VCRC;

public static class EntropyAnalysisExtensions
{
    /// <summary>
    ///     Performs VCRC entropy analysis over a range
    ///     of indoor and outdoor temperatures.
    /// </summary>
    /// <param name="cycles">List of VCRCs.</param>
    /// <param name="indoor">List of indoor temperatures.</param>
    /// <param name="outdoor">List of outdoor temperatures.</param>
    /// <returns>
    ///     Result of the VCRC entropy analysis in range of temperatures.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     The lists should have the same length!
    /// </exception>
    public static IEntropyAnalysisResult EntropyAnalysis(
        this IList<IEntropyAnalysable> cycles,
        IList<Temperature> indoor,
        IList<Temperature> outdoor
    ) =>
        cycles.Count == indoor.Count && indoor.Count == outdoor.Count
            ? cycles
                .Select(
                    (cycle, i) => cycle.EntropyAnalysis(indoor[i], outdoor[i])
                )
                .ToList()
                .Average()
            : throw new ArgumentException(
                "The lists should have the same length!"
            );

    /// <summary>
    ///     Computes the average of a list of the entropy analysis results.
    /// </summary>
    /// <param name="results">List of the entropy analysis results.</param>
    /// <returns>The average.</returns>
    public static IEntropyAnalysisResult Average(
        this IList<IEntropyAnalysisResult> results
    ) =>
        new EntropyAnalysisResult(
            results
                .Select(i => i.ThermodynamicPerfection.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.MinSpecificWorkRatio.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.CompressorEnergyLossRatio.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.CondenserEnergyLossRatio.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.GasCoolerEnergyLossRatio.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.ExpansionValvesEnergyLossRatio.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.EjectorEnergyLossRatio.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.EvaporatorEnergyLossRatio.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.RecuperatorEnergyLossRatio.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.EconomizerEnergyLossRatio.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.MixingEnergyLossRatio.Percent)
                .Average()
                .Percent(),
            results
                .Select(i => i.AnalysisRelativeError.Percent)
                .Average()
                .Percent()
        );
}
