namespace VCRC;

public interface IEntropyAnalysable
{
    /// <summary>
    ///     Performs entropy analysis of the VCRC.
    /// </summary>
    /// <param name="indoor">Indoor temperature.</param>
    /// <param name="outdoor">Outdoor temperature.</param>
    /// <returns>Result of the VCRC entropy analysis.</returns>
    /// <exception cref="ValidationException">
    ///     Indoor and outdoor temperatures should not be equal!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference in the evaporator!
    ///     Increase 'cold' source temperature.
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference in the condenser or gas cooler!
    ///     Decrease 'hot' source temperature.
    /// </exception>
    public EntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    );
}
