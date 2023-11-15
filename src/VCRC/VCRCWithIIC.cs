namespace VCRC;

/// <inheritdoc cref="IVCRCWithIIC"/>
public class VCRCWithIIC : AbstractTwoStageVCRC, IVCRCWithIIC
{
    /// <inheritdoc cref="VCRCWithIIC"/>
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
    public VCRCWithIIC(
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
        Point6 = Point5.IsenthalpicExpansionTo(IntermediatePressure);
        Point7 = Refrigerant.DewPointAt(IntermediatePressure);
        Point8 = Refrigerant.BubblePointAt(IntermediatePressure);
        Point9 = Point8.IsenthalpicExpansionTo(Evaporator.Pressure);
        Point3 = Refrigerant.Mixing(
            EvaporatorSpecificMassFlow,
            Point2,
            IntermediateSpecificMassFlow,
            Point7
        );
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point4 = Point3.CompressionTo(
            HeatReleaser.Pressure,
            Compressor.Efficiency
        );
    }

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point9, Point1),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point4s, Point5),
            new EVNode(HeatReleaserSpecificMassFlow, Point5, Point6),
            new EVNode(EvaporatorSpecificMassFlow, Point8, Point9),
            null,
            null,
            null,
            null,
            new MixingNode(
                Point3,
                EvaporatorSpecificMassFlow,
                Point2,
                IntermediateSpecificMassFlow,
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

    public IRefrigerant Point9 { get; }

    public sealed override Pressure IntermediatePressure =>
        base.IntermediatePressure;

    public sealed override Ratio IntermediateSpecificMassFlow =>
        base.IntermediateSpecificMassFlow;

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow
        / (1 - Point6.Quality!.Value.DecimalFractions);

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy
        - Point1.Enthalpy
        + HeatReleaserSpecificMassFlow.DecimalFractions
            * (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point9.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions
        * (Point4.Enthalpy - Point5.Enthalpy);

    public override IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) => Analyzer.PerformAnalysis(indoor, outdoor);
}
