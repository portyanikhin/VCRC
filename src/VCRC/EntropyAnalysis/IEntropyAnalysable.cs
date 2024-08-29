namespace VCRC;

/// <summary>
///     Supports performing entropy analysis.
/// </summary>
public interface IEntropyAnalysable
{
    /// <summary>
    ///     Performs entropy analysis of the VCRC.
    /// </summary>
    /// <param name="indoor">Indoor temperature.</param>
    /// <param name="outdoor">Outdoor temperature.</param>
    /// <returns>Result of the VCRC entropy analysis.</returns>
    /// <exception cref="ArgumentException">
    ///     Indoor and outdoor temperatures should not be equal!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Wrong temperature difference in the evaporator! Increase 'cold' source temperature.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Wrong temperature difference in the condenser or gas cooler!
    ///     Decrease 'hot' source temperature.
    /// </exception>
    IEntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor);
}
