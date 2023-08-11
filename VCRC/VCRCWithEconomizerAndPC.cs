namespace VCRC;

/// <summary>
///     Two-stage VCRC with economizer and parallel compression.
/// </summary>
public class VCRCWithEconomizerAndPC
    : AbstractTwoStageVCRC,
        IVCRCWithEconomizerAndPC
{
    /// <summary>
    ///     Two-stage VCRC with economizer and parallel compression.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature
    ///     should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at the economizer 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at the economizer 'cold' side!
    /// </exception>
    public VCRCWithEconomizerAndPC(
        IEvaporator evaporator,
        ICompressor compressor,
        IHeatReleaser heatReleaser,
        IEconomizer economizer
    )
        : base(evaporator, compressor, heatReleaser)
    {
        Economizer = economizer;
        Point2s = Point1.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point2 = Point1.CompressionTo(
            HeatReleaser.Pressure,
            Compressor.Efficiency
        );
        Point7 = Point6.IsenthalpicExpansionTo(IntermediatePressure);
        Point3 = Refrigerant.Superheated(
            IntermediatePressure,
            Economizer.Superheat
        );
        new VCRCWithEconomizerAndPCValidator().ValidateAndThrow(this);
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point4 = Point3.CompressionTo(
            HeatReleaser.Pressure,
            Compressor.Efficiency
        );
        Point8 = Point6.CoolingTo(
            Point7.Temperature + Economizer.TemperatureDifference
        );
        Point9 = Point8.IsenthalpicExpansionTo(Evaporator.Pressure);
        Point5s = Refrigerant.Mixing(
            EvaporatorSpecificMassFlow,
            Point2s,
            IntermediateSpecificMassFlow,
            Point4s
        );
        Point5 = Refrigerant.Mixing(
            EvaporatorSpecificMassFlow,
            Point2,
            IntermediateSpecificMassFlow,
            Point4
        );
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private IRefrigerant Point5s { get; }

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point9, Point1),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point5s, Point6),
            new EVNode(IntermediateSpecificMassFlow, Point6, Point7),
            new EVNode(EvaporatorSpecificMassFlow, Point8, Point9),
            null,
            null,
            null,
            new EconomizerNode(
                IntermediateSpecificMassFlow,
                Point7,
                Point3,
                EvaporatorSpecificMassFlow,
                Point6,
                Point8
            ),
            new MixingNode(
                Point5,
                EvaporatorSpecificMassFlow,
                Point2,
                IntermediateSpecificMassFlow,
                Point4
            )
        );

    public IEconomizer Economizer { get; }

    public IRefrigerant Point1 => Evaporator.Outlet;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point2s { get; }

    public IRefrigerant Point2 { get; }

    public IRefrigerant Point3 { get; }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point4s { get; }

    public IRefrigerant Point4 { get; }

    public IRefrigerant Point5 { get; }

    public IRefrigerant Point6 => HeatReleaser.Outlet;

    public IRefrigerant Point7 { get; }

    public IRefrigerant Point8 { get; }

    public IRefrigerant Point9 { get; }

    public sealed override Pressure IntermediatePressure =>
        base.IntermediatePressure;

    public sealed override Ratio IntermediateSpecificMassFlow =>
        base.IntermediateSpecificMassFlow;

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow
        * (
            1
            + (Point6.Enthalpy - Point8.Enthalpy)
                / (Point3.Enthalpy - Point7.Enthalpy)
        );

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy
        - Point1.Enthalpy
        + IntermediateSpecificMassFlow.DecimalFractions
            * (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point9.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions
        * (Point5.Enthalpy - Point6.Enthalpy);

    public override IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) => Analyzer.PerformAnalysis(indoor, outdoor);
}
