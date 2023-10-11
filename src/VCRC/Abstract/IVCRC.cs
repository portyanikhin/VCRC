namespace VCRC;

/// <summary>
///     Vapor-compression refrigeration cycle.
/// </summary>
public interface IVCRC : IEntropyAnalysable
{
    /// <summary>
    ///     Evaporator.
    /// </summary>
    public IEvaporator Evaporator { get; }

    /// <summary>
    ///     Compressor.
    /// </summary>
    public ICompressor Compressor { get; }

    /// <summary>
    ///     Heat releaser (condenser for subcritical VCRC or
    ///     gas cooler for transcritical VCRC).
    /// </summary>
    public IHeatReleaser HeatReleaser { get; }

    /// <summary>
    ///     Condenser (not <c>null</c> for subcritical VCRC).
    /// </summary>
    public ICondenser? Condenser { get; }

    /// <summary>
    ///     Gas cooler (not <c>null</c> for transcritical VCRC).
    /// </summary>
    public IHeatReleaser? GasCooler { get; }

    /// <summary>
    ///     <c>true</c> if VCRC is transcritical,
    ///     <c>false</c> if VCRC is subcritical.
    /// </summary>
    public bool IsTranscritical { get; }

    /// <summary>
    ///     Specific mass flow rate of the evaporator.
    /// </summary>
    public Ratio EvaporatorSpecificMassFlow { get; }

    /// <summary>
    ///     Specific mass flow rate of the condenser or gas cooler.
    /// </summary>
    public Ratio HeatReleaserSpecificMassFlow { get; }

    /// <summary>
    ///     Specific work of isentropic compression.
    /// </summary>
    public SpecificEnergy IsentropicSpecificWork { get; }

    /// <summary>
    ///     Specific work of real compression.
    /// </summary>
    public SpecificEnergy SpecificWork { get; }

    /// <summary>
    ///     Specific cooling capacity of the cycle.
    /// </summary>
    public SpecificEnergy SpecificCoolingCapacity { get; }

    /// <summary>
    ///     Specific heating capacity of the cycle.
    /// </summary>
    public SpecificEnergy SpecificHeatingCapacity { get; }

    /// <summary>
    ///     Energy efficiency ratio, aka cooling coefficient.
    /// </summary>
    public double EER { get; }

    /// <summary>
    ///     Coefficient of performance, aka heating coefficient.
    /// </summary>
    public double COP { get; }
}
