namespace VCRC;

/// <inheritdoc cref="IVCRCWithEconomizerAndTPI"/>
public class VCRCWithEconomizerAndTPI : AbstractTwoStageVCRC, IVCRCWithEconomizerAndTPI
{
    /// <inheritdoc cref="VCRCWithEconomizerAndTPI"/>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at the economizer 'cold' side!
    /// </exception>
    public VCRCWithEconomizerAndTPI(
        IEvaporator evaporator,
        ICompressor compressor,
        IHeatReleaser heatReleaser,
        IAuxiliaryHeatExchanger economizer
    )
        : base(evaporator, compressor, heatReleaser)
    {
        Economizer = economizer;
        Point2s = Point1.IsentropicCompressionTo(IntermediatePressure);
        Point2 = Point1.CompressionTo(IntermediatePressure, Compressor.Efficiency);
        Point3 = Refrigerant.DewPointAt(IntermediatePressure);
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point4 = Point3.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
        Point6 = Point5.IsenthalpicExpansionTo(IntermediatePressure);
        new VCRCWithEconomizerAndTPIValidator().ValidateAndThrow(this);
        Point8 = Point5.CoolingTo(Point6.Temperature + Economizer.TemperatureDifference);
        Point7 = Point6.HeatingTo(
            (
                (
                    Point6.Enthalpy.JoulesPerKilogram
                        * (Point2.Enthalpy.JoulesPerKilogram - Point3.Enthalpy.JoulesPerKilogram)
                    + Point3.Enthalpy.JoulesPerKilogram
                        * (Point5.Enthalpy.JoulesPerKilogram - Point8.Enthalpy.JoulesPerKilogram)
                )
                / (
                    Point2.Enthalpy.JoulesPerKilogram
                    - Point3.Enthalpy.JoulesPerKilogram
                    + Point5.Enthalpy.JoulesPerKilogram
                    - Point8.Enthalpy.JoulesPerKilogram
                )
            ).JoulesPerKilogram()
        );
        Point9 = Point8.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point9, Point1),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point4s, Point5),
            new EVNode(IntermediateSpecificMassFlow, Point5, Point6),
            new EVNode(EvaporatorSpecificMassFlow, Point8, Point9),
            null,
            null,
            null,
            new EconomizerNode(
                IntermediateSpecificMassFlow,
                Point6,
                Point7,
                EvaporatorSpecificMassFlow,
                Point5,
                Point8
            ),
            new MixingNode(
                Point3,
                EvaporatorSpecificMassFlow,
                Point2,
                IntermediateSpecificMassFlow,
                Point7
            )
        );

    public IAuxiliaryHeatExchanger Economizer { get; }
    public IRefrigerant Point1 => Evaporator.Outlet;
    public IRefrigerant Point2s { get; }
    public IRefrigerant Point2 { get; }
    public IRefrigerant Point3 { get; }
    public IRefrigerant Point4s { get; }
    public IRefrigerant Point4 { get; }
    public IRefrigerant Point5 => HeatReleaser.Outlet;
    public IRefrigerant Point6 { get; }
    public IRefrigerant Point7 { get; }
    public IRefrigerant Point8 { get; }
    public IRefrigerant Point9 { get; }
    public sealed override Pressure IntermediatePressure => base.IntermediatePressure;
    public sealed override Ratio IntermediateSpecificMassFlow => base.IntermediateSpecificMassFlow;

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow
        * (1 + (Point2.Enthalpy - Point3.Enthalpy) / (Point3.Enthalpy - Point7.Enthalpy));

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy
        - Point1.Enthalpy
        + HeatReleaserSpecificMassFlow.DecimalFractions * (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point9.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions * (Point4.Enthalpy - Point5.Enthalpy);

    public override IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) => Analyzer.PerformAnalysis(indoor, outdoor);
}
