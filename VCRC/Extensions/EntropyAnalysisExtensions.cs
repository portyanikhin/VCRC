using System;
using System.Collections.Generic;
using System.Linq;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;

namespace VCRC.Extensions;

public static class EntropyAnalysisExtensions
{
    /// <summary>
    ///     Performs entropy analysis of the VCRC in range of temperatures.
    /// </summary>
    /// <param name="cycles">List of VCRCs.</param>
    /// <param name="indoor">List of indoor temperatures.</param>
    /// <param name="outdoor">List of outdoor temperatures.</param>
    /// <returns>Result of the VCRC entropy analysis in range of temperatures.</returns>
    /// <exception cref="ArgumentException">The lists should have the same length!</exception>
    public static EntropyAnalysisResult EntropyAnalysis(
        this List<IEntropyAnalysable> cycles, List<Temperature> indoor, List<Temperature> outdoor) =>
        cycles.Count == indoor.Count && indoor.Count == outdoor.Count
            ? cycles.Select((с, i) => с.EntropyAnalysis(indoor[i], outdoor[i])).ToList().Average()
            : throw new ArgumentException("The lists should have the same length!");

    /// <summary>
    ///     Computes the average of a list of <c>EntropyAnalysisResult</c> values.
    /// </summary>
    /// <param name="results">List of <c>EntropyAnalysisResult</c> values.</param>
    /// <returns>The average.</returns>
    public static EntropyAnalysisResult Average(this List<EntropyAnalysisResult> results) =>
        new(results.Select(i => i.ThermodynamicPerfection.Percent).Average().Percent(),
            results.Select(i => i.MinSpecificWorkRatio.Percent).Average().Percent(),
            results.Select(i => i.CompressorEnergyLossRatio.Percent).Average().Percent(),
            results.Select(i => i.CondenserEnergyLossRatio.Percent).Average().Percent(),
            results.Select(i => i.ExpansionValvesEnergyLossRatio.Percent).Average().Percent(),
            results.Select(i => i.EvaporatorEnergyLossRatio.Percent).Average().Percent(),
            results.Select(i => i.RecuperatorEnergyLossRatio.Percent).Average().Percent(),
            results.Select(i => i.EconomizerEnergyLossRatio.Percent).Average().Percent(),
            results.Select(i => i.MixingEnergyLossRatio.Percent).Average().Percent(),
            results.Select(i => i.AnalysisRelativeError.Percent).Average().Percent());
}