namespace VCRC;

/// <summary>
///     Two-stage VCRC with complete intercooling.
/// </summary>
public class VCRCWithCIC : AbstractTwoStageVCRC, IVCRCWithCIC
{
    /// <summary>
    ///     Two-stage VCRC with complete intercooling.
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
    /// <exception cref="ValidationException">
    ///     Refrigerant should be a single component or an azeotropic blend!
    /// </exception>
    public VCRCWithCIC(
        IEvaporator evaporator,
        ICompressor compressor,
        IHeatReleaser heatReleaser
    )
        : base(evaporator, compressor, heatReleaser)
    {
        new RefrigerantTypeValidator().ValidateAndThrow(Refrigerant);
        Point2s = Point1.IsentropicCompressionTo(IntermediatePressure);
        Point2 = Point1.CompressionTo(
            IntermediatePressure,
            Compressor.Efficiency
        );
        Point3 = Refrigerant.DewPointAt(IntermediatePressure);
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point6 = Point5.IsenthalpicExpansionTo(IntermediatePressure);
        Point7 = Refrigerant.BubblePointAt(IntermediatePressure);
        Point8 = Point7.IsenthalpicExpansionTo(Evaporator.Pressure);
        Point4 = Point3.CompressionTo(
            HeatReleaser.Pressure,
            Compressor.Efficiency
        );
    }

    private Ratio BarbotageSpecificMassFlow =>
        EvaporatorSpecificMassFlow
        * (
            (Point2.Enthalpy - Point3.Enthalpy)
            / (Point3.Enthalpy - Point7.Enthalpy)
        );

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point8, Point1),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point4s, Point5),
            new EVNode(HeatReleaserSpecificMassFlow, Point5, Point6),
            new EVNode(EvaporatorSpecificMassFlow, Point7, Point8),
            null,
            null,
            null,
            null,
            new MixingNode(
                Point3,
                EvaporatorSpecificMassFlow,
                Point2,
                BarbotageSpecificMassFlow,
                Point7
            )
        );

    public IRefrigerant Point1 => Evaporator.Outlet;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point2s { get; }

    public IRefrigerant Point2 { get; }

    public IRefrigerant Point3 { get; }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point4s { get; }

    public IRefrigerant Point4 { get; }

    public IRefrigerant Point5 => HeatReleaser.Outlet;

    public IRefrigerant Point6 { get; }

    public IRefrigerant Point7 { get; }

    public IRefrigerant Point8 { get; }

    public sealed override Pressure IntermediatePressure =>
        base.IntermediatePressure;

    public sealed override Ratio IntermediateSpecificMassFlow =>
        HeatReleaserSpecificMassFlow;

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        (EvaporatorSpecificMassFlow + BarbotageSpecificMassFlow)
        / (1 - Point6.Quality!.Value.DecimalFractions);

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy
        - Point1.Enthalpy
        + HeatReleaserSpecificMassFlow.DecimalFractions
            * (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point8.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions
        * (Point4.Enthalpy - Point5.Enthalpy);

    public override IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) => Analyzer.PerformAnalysis(indoor, outdoor);
}
