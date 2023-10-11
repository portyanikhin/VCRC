namespace VCRC;

/// <summary>
///     Two-stage VCRC with an ejector as an expansion device,
///     economizer and parallel compression.
/// </summary>
public class VCRCWithEjectorEconomizerAndPC
    : AbstractTwoStageVCRC,
        IVCRCWithEjectorEconomizerAndPC
{
    private Pressure _diffuserOutletPressure;
    private IEjectorFlows _ejectorFlows = default!;

    /// <summary>
    ///     Two-stage VCRC with an ejector as an expansion device,
    ///     economizer and parallel compression.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <param name="ejector">Ejector.</param>
    /// <param name="economizer">Economizer.</param>
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
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at the economizer 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at the economizer 'cold' side!
    /// </exception>
    public VCRCWithEjectorEconomizerAndPC(
        IEvaporator evaporator,
        ICompressor compressor,
        IHeatReleaser heatReleaser,
        IEjector ejector,
        IEconomizer economizer
    )
        : base(evaporator, compressor, heatReleaser)
    {
        new RefrigerantTypeValidator().ValidateAndThrow(Refrigerant);
        Ejector = ejector;
        Economizer = economizer;
        CalculateDiffuserOutletPressure();
        Point1 = Refrigerant.DewPointAt(_ejectorFlows.DiffuserOutlet.Pressure);
        Point2s = Point1.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point2 = Point1.CompressionTo(
            HeatReleaser.Pressure,
            Compressor.Efficiency
        );
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point4 = Point3.CompressionTo(
            HeatReleaser.Pressure,
            Compressor.Efficiency
        );
        Point5s = Refrigerant.Mixing(
            HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
            Point2s,
            IntermediateSpecificMassFlow,
            Point4s
        );
        Point5 = Refrigerant.Mixing(
            HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
            Point2,
            IntermediateSpecificMassFlow,
            Point4
        );
        Point12 = Refrigerant.BubblePointAt(
            _ejectorFlows.DiffuserOutlet.Pressure
        );
        Point13 = Point12.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private IRefrigerant Point5s { get; }

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point13, Point14),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point5s, Point6),
            new EVNode(IntermediateSpecificMassFlow, Point6, Point7),
            new EVNode(EvaporatorSpecificMassFlow, Point12, Point13),
            null,
            new EjectorNode(
                Point11,
                HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
                Point8,
                EvaporatorSpecificMassFlow,
                Point14
            ),
            null,
            new EconomizerNode(
                IntermediateSpecificMassFlow,
                Point7,
                Point3,
                HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
                Point6,
                Point8
            ),
            new MixingNode(
                Point5,
                HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
                Point2,
                IntermediateSpecificMassFlow,
                Point4
            )
        );

    public IEjector Ejector { get; }

    public IEconomizer Economizer { get; }

    public IRefrigerant Point1 { get; }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point2s { get; }

    public IRefrigerant Point2 { get; }

    public IRefrigerant Point3 { get; private set; } = default!;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point4s { get; }

    public IRefrigerant Point4 { get; }

    public IRefrigerant Point5 { get; }

    public IRefrigerant Point6 => HeatReleaser.Outlet;

    public IRefrigerant Point7 { get; private set; } = default!;

    public IRefrigerant Point8 { get; private set; } = default!;

    public IRefrigerant Point9 => _ejectorFlows.NozzleOutlet;

    public IRefrigerant Point10 => _ejectorFlows.MixingInlet;

    public IRefrigerant Point11 => _ejectorFlows.DiffuserOutlet;

    public IRefrigerant Point12 { get; }

    public IRefrigerant Point13 { get; }

    public IRefrigerant Point14 => Evaporator.Outlet;

    public IRefrigerant Point15 => _ejectorFlows.SuctionOutlet;

    public sealed override Pressure IntermediatePressure =>
        CalculateIntermediatePressure(
            _diffuserOutletPressure,
            HeatReleaser.Pressure
        );

    public sealed override Ratio IntermediateSpecificMassFlow =>
        HeatReleaserSpecificMassFlow
        - EvaporatorSpecificMassFlow
            * (
                Point11.Quality!.Value.DecimalFractions
                / (1 - Point11.Quality!.Value.DecimalFractions)
            );

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow
        * (
            Point11.Quality!.Value.DecimalFractions
            / (1 - Point11.Quality!.Value.DecimalFractions)
        )
        * (
            1
            + (Point6.Enthalpy - Point8.Enthalpy)
                / (Point3.Enthalpy - Point7.Enthalpy)
        );

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        (
            HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow
        ).DecimalFractions * (Point2s.Enthalpy - Point1.Enthalpy)
        + IntermediateSpecificMassFlow.DecimalFractions
            * (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point14.Enthalpy - Point13.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions
        * (Point5.Enthalpy - Point6.Enthalpy);

    public override IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) => Analyzer.PerformAnalysis(indoor, outdoor);

    private void CalculateDiffuserOutletPressure()
    {
        NewtonRaphson.FindRootNearGuess(
            ToSolve,
            Differentiate.FirstDerivativeFunc(ToSolve),
            Evaporator.Pressure.Pascals + 100,
            Evaporator.Pressure.Pascals + 1,
            HeatReleaser.Pressure.Pascals - 1,
            10
        );
        return;

        double ToSolve(double diffuserOutletPressure)
        {
            _diffuserOutletPressure = diffuserOutletPressure.Pascals();
            Point7 = Point6.IsenthalpicExpansionTo(IntermediatePressure);
            Point3 = Refrigerant.Superheated(
                IntermediatePressure,
                Economizer.Superheat
            );
            new VCRCWithEjectorEconomizerAndPCValidator().ValidateAndThrow(
                this
            );
            Point8 = Point6.CoolingTo(
                Point7.Temperature + Economizer.TemperatureDifference
            );
            _ejectorFlows = Ejector.CalculateFlows(Point8, Point14);
            return (
                _ejectorFlows.DiffuserOutlet.Pressure - _diffuserOutletPressure
            ).Pascals;
        }
    }
}
