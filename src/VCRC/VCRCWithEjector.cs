namespace VCRC;

/// <inheritdoc cref="IVCRCWithEjector"/>
public class VCRCWithEjector : AbstractVCRC, IVCRCWithEjector
{
    private readonly IEjectorFlows _ejectorFlows;

    /// <inheritdoc cref="VCRCWithEjector"/>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <param name="ejector">Ejector.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature
    ///     should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Refrigerant should be a single component or an azeotropic blend!
    /// </exception>
    public VCRCWithEjector(
        IEvaporator evaporator,
        ICompressor compressor,
        IHeatReleaser heatReleaser,
        IEjector ejector
    )
        : base(evaporator, compressor, heatReleaser)
    {
        new RefrigerantTypeValidator().ValidateAndThrow(Refrigerant);
        Ejector = ejector;
        _ejectorFlows = Ejector.CalculateFlows(Point3, Point9);
        Point1 = Refrigerant.DewPointAt(Point6.Pressure);
        Point2s = Point1.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point2 = Point1.CompressionTo(
            HeatReleaser.Pressure,
            Compressor.Efficiency
        );
        Point7 = Refrigerant.BubblePointAt(Point6.Pressure);
        Point8 = Point7.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point8, Point9),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point2s, Point3),
            new EVNode(EvaporatorSpecificMassFlow, Point7, Point8),
            null,
            null,
            new EjectorNode(
                Point6,
                HeatReleaserSpecificMassFlow,
                Point3,
                EvaporatorSpecificMassFlow,
                Point9
            )
        );

    public IEjector Ejector { get; }

    public IRefrigerant Point1 { get; }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point2s { get; }

    public IRefrigerant Point2 { get; }

    public IRefrigerant Point3 => HeatReleaser.Outlet;

    public IRefrigerant Point4 => _ejectorFlows.NozzleOutlet;

    public IRefrigerant Point5 => _ejectorFlows.MixingInlet;

    public IRefrigerant Point6 => _ejectorFlows.DiffuserOutlet;

    public IRefrigerant Point7 { get; }

    public IRefrigerant Point8 { get; }

    public IRefrigerant Point9 => Evaporator.Outlet;

    public IRefrigerant Point10 => _ejectorFlows.SuctionOutlet;

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow
        * (
            Point6.Quality!.Value.DecimalFractions
            / (1 - Point6.Quality!.Value.DecimalFractions)
        );

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        HeatReleaserSpecificMassFlow.DecimalFractions
        * (Point2s.Enthalpy - Point1.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point9.Enthalpy - Point8.Enthalpy;

    public override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions
        * (Point2.Enthalpy - Point3.Enthalpy);

    public override IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) => Analyzer.PerformAnalysis(indoor, outdoor);
}
