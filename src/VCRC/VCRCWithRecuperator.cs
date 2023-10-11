namespace VCRC;

/// <summary>
///     Single-stage VCRC with recuperator.
/// </summary>
public class VCRCWithRecuperator : AbstractVCRC, IVCRCWithRecuperator
{
    /// <summary>
    ///     Single-stage VCRC with recuperator.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="recuperator">Recuperator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature
    ///     should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at the recuperator 'hot' side!
    /// </exception>
    public VCRCWithRecuperator(
        IEvaporator evaporator,
        IAuxiliaryHeatExchanger recuperator,
        ICompressor compressor,
        IHeatReleaser heatReleaser
    )
        : base(evaporator, compressor, heatReleaser)
    {
        Recuperator = recuperator;
        new VCRCWithRecuperatorValidator().ValidateAndThrow(this);
        Point2 = Point1.HeatingTo(
            Point4.Temperature - Recuperator.TemperatureDifference
        );
        Point3s = Point2.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point3 = Point2.CompressionTo(
            HeatReleaser.Pressure,
            Compressor.Efficiency
        );
        Point5 = Point4.CoolingTo(
            Point4.Enthalpy - (Point2.Enthalpy - Point1.Enthalpy)
        );
        Point6 = Point5.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point6, Point1),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point3s, Point4),
            new EVNode(HeatReleaserSpecificMassFlow, Point5, Point6),
            null,
            null,
            null,
            new RecuperatorNode(
                EvaporatorSpecificMassFlow,
                Point1,
                Point2,
                HeatReleaserSpecificMassFlow,
                Point4,
                Point5
            )
        );

    public IAuxiliaryHeatExchanger Recuperator { get; }

    public IRefrigerant Point1 => Evaporator.Outlet;

    public IRefrigerant Point2 { get; }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point3s { get; }

    public IRefrigerant Point3 { get; }

    public IRefrigerant Point4 => HeatReleaser.Outlet;

    public IRefrigerant Point5 { get; }

    public IRefrigerant Point6 { get; }

    public sealed override Ratio HeatReleaserSpecificMassFlow { get; } =
        100.Percent();

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point3s.Enthalpy - Point2.Enthalpy;

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point6.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        Point3.Enthalpy - Point4.Enthalpy;

    public override IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) => Analyzer.PerformAnalysis(indoor, outdoor);
}
