namespace VCRC;

/// <inheritdoc cref="ISimpleVCRC"/>
public class SimpleVCRC : AbstractVCRC, ISimpleVCRC
{
    /// <inheritdoc cref="SimpleVCRC"/>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    public SimpleVCRC(IEvaporator evaporator, ICompressor compressor, IHeatReleaser heatReleaser)
        : base(evaporator, compressor, heatReleaser)
    {
        Point2s = Point1.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point2 = Point1.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
        Point4 = Point3.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point4, Point1),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point2s, Point3),
            new EVNode(HeatReleaserSpecificMassFlow, Point3, Point4)
        );

    public IRefrigerant Point1 => Evaporator.Outlet;
    public IRefrigerant Point2s { get; }
    public IRefrigerant Point2 { get; }
    public IRefrigerant Point3 => HeatReleaser.Outlet;
    public IRefrigerant Point4 { get; }
    public sealed override Ratio HeatReleaserSpecificMassFlow { get; } = 100.Percent();

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy - Point1.Enthalpy;

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point4.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        Point2.Enthalpy - Point3.Enthalpy;

    public override IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) => Analyzer.PerformAnalysis(indoor, outdoor);
}
