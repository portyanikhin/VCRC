using System;
using UnitsNet;

namespace VCRC;

public interface IEntropyAnalysable
{
    /// <summary>
    ///     Performs entropy analysis of the VCRC.
    /// </summary>
    /// <param name="indoor">Indoor temperature.</param>
    /// <param name="outdoor">Outdoor temperature.</param>
    /// <returns>Result of the VCRC entropy analysis.</returns>
    /// <exception cref="ArgumentException">Indoor and outdoor temperatures should not be equal!</exception>
    /// <exception cref="ArgumentException">
    ///     Wrong temperature difference in the evaporator! Increase 'cold' source temperature.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Wrong temperature difference in the condenser or gas cooler! Decrease 'hot' source temperature.
    /// </exception>
    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor);

    protected static (Temperature, Temperature) SourceTemperatures(Temperature indoor, Temperature outdoor,
        Temperature coldSourceMin, Temperature hotSourceMax)
    {
        if (indoor == outdoor) throw new ArgumentException("Indoor and outdoor temperatures should not be equal!");
        var (coldSource, hotSource) = (UnitMath.Min(indoor, outdoor), UnitMath.Max(indoor, outdoor));
        if (coldSource <= coldSourceMin)
            throw new ArgumentException(
                "Wrong temperature difference in the evaporator! Increase 'cold' source temperature.");
        if (hotSource >= hotSourceMax)
            throw new ArgumentException(
                "Wrong temperature difference in the condenser or gas cooler! Decrease 'hot' source temperature.");
        return (coldSource, hotSource);
    }
}