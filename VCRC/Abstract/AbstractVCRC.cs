namespace VCRC;

/// <summary>
///     Vapor-compression refrigeration cycle.
/// </summary>
public abstract class AbstractVCRC : IVCRC
{
    /// <summary>
    ///     Vapor-compression refrigeration cycle.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature
    ///     should be greater than evaporating temperature!
    /// </exception>
    protected AbstractVCRC(
        IEvaporator evaporator,
        ICompressor compressor,
        IHeatReleaser heatReleaser
    )
    {
        Evaporator = evaporator;
        Compressor = compressor;
        HeatReleaser = heatReleaser;
        new VCRCValidator().ValidateAndThrow(this);
        Refrigerant = new Refrigerant(Evaporator.RefrigerantName);
    }

    protected IRefrigerant Refrigerant { get; }

    public IEvaporator Evaporator { get; }

    public ICompressor Compressor { get; }

    public IHeatReleaser HeatReleaser { get; }

    public ICondenser? Condenser => HeatReleaser as Condenser;

    public IHeatReleaser? GasCooler => HeatReleaser as GasCooler;

    public bool IsTranscritical => GasCooler is not null;

    public Ratio EvaporatorSpecificMassFlow => 100.Percent();

    public abstract Ratio HeatReleaserSpecificMassFlow { get; }

    public abstract SpecificEnergy IsentropicSpecificWork { get; }

    public SpecificEnergy SpecificWork =>
        IsentropicSpecificWork / Compressor.Efficiency.DecimalFractions;

    public abstract SpecificEnergy SpecificCoolingCapacity { get; }

    public abstract SpecificEnergy SpecificHeatingCapacity { get; }

    public double EER => SpecificCoolingCapacity / SpecificWork;

    public double COP => SpecificHeatingCapacity / SpecificWork;

    public abstract IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    );
}
