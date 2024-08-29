namespace VCRC;

/// <summary>
///     Vapor-compression refrigeration cycle.
/// </summary>
public interface IVCRC : IEntropyAnalysable
{
    /// <summary>
    ///     Evaporator.
    /// </summary>
    IEvaporator Evaporator { get; }

    /// <summary>
    ///     Compressor.
    /// </summary>
    ICompressor Compressor { get; }

    /// <summary>
    ///     Heat releaser (condenser for subcritical VCRC or gas cooler for transcritical VCRC).
    /// </summary>
    IHeatReleaser HeatReleaser { get; }

    /// <summary>
    ///     Condenser (not <c>null</c> for subcritical VCRC).
    /// </summary>
    ICondenser? Condenser { get; }

    /// <summary>
    ///     Gas cooler (not <c>null</c> for transcritical VCRC).
    /// </summary>
    IHeatReleaser? GasCooler { get; }

    /// <summary>
    ///     <c>true</c> if VCRC is transcritical, <c>false</c> if VCRC is subcritical.
    /// </summary>
    bool IsTranscritical { get; }

    /// <summary>
    ///     Specific mass flow rate of the evaporator.
    /// </summary>
    Ratio EvaporatorSpecificMassFlow { get; }

    /// <summary>
    ///     Specific mass flow rate of the condenser or gas cooler.
    /// </summary>
    Ratio HeatReleaserSpecificMassFlow { get; }

    /// <summary>
    ///     Specific work of isentropic compression.
    /// </summary>
    SpecificEnergy IsentropicSpecificWork { get; }

    /// <summary>
    ///     Specific work of real compression.
    /// </summary>
    SpecificEnergy SpecificWork { get; }

    /// <summary>
    ///     Specific cooling capacity of the cycle.
    /// </summary>
    SpecificEnergy SpecificCoolingCapacity { get; }

    /// <summary>
    ///     Specific heating capacity of the cycle.
    /// </summary>
    SpecificEnergy SpecificHeatingCapacity { get; }

    /// <summary>
    ///     Energy efficiency ratio, aka cooling coefficient.
    /// </summary>
    double EER { get; }

    /// <summary>
    ///     Coefficient of performance, aka heating coefficient.
    /// </summary>
    double COP { get; }
}
