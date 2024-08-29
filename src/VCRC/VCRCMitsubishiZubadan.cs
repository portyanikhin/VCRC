namespace VCRC;

/// <inheritdoc cref="IVCRCMitsubishiZubadan"/>
public class VCRCMitsubishiZubadan : AbstractTwoStageVCRC, IVCRCMitsubishiZubadan
{
    /// <inheritdoc cref="VCRCMitsubishiZubadan"/>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="condenser">Condenser.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ArgumentException">Solution not found!</exception>
    /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     There should be a two-phase refrigerant at the recuperator 'hot' inlet!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at the recuperator 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at the recuperator 'cold' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at the economizer 'cold' side!
    /// </exception>
    public VCRCMitsubishiZubadan(
        IEvaporator evaporator,
        ICompressor compressor,
        ICondenser condenser,
        IAuxiliaryHeatExchanger economizer
    )
        : base(evaporator, compressor, condenser)
    {
        Condenser = condenser;
        Economizer = economizer;
        CalculateInjectionQuality();
        new VCRCMitsubishiZubadanValidator().ValidateAndThrow(this);
        Point5s = Point4!.IsentropicCompressionTo(Condenser.Pressure);
        Point5 = Point4.CompressionTo(Condenser.Pressure, Compressor.Efficiency);
        Recuperator = new Recuperator(Point7!.Temperature - Point2!.Temperature);
    }

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point12, Point1),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point5s, Point6),
            new EVNode(HeatReleaserSpecificMassFlow, Point6, Point7),
            new EVNode(IntermediateSpecificMassFlow, Point8, Point9),
            new EVNode(EvaporatorSpecificMassFlow, Point11, Point12),
            null,
            new RecuperatorNode(
                EvaporatorSpecificMassFlow,
                Point1,
                Point2,
                HeatReleaserSpecificMassFlow,
                Point7,
                Point8
            ),
            new EconomizerNode(
                IntermediateSpecificMassFlow,
                Point9,
                Point10,
                EvaporatorSpecificMassFlow,
                Point8,
                Point11
            ),
            new MixingNode(
                Point4,
                EvaporatorSpecificMassFlow,
                Point3,
                IntermediateSpecificMassFlow,
                Point10
            )
        );

    public new ICondenser Condenser { get; }
    public IAuxiliaryHeatExchanger Recuperator { get; }
    public IAuxiliaryHeatExchanger Economizer { get; }
    public Pressure RecuperatorHighPressure { get; private set; }
    public IRefrigerant Point1 => Evaporator.Outlet;
    public IRefrigerant Point2 { get; private set; }
    public IRefrigerant Point3s { get; private set; } = default!;
    public IRefrigerant Point3 { get; private set; } = default!;
    public IRefrigerant Point4 { get; private set; }
    public IRefrigerant Point5s { get; }
    public IRefrigerant Point5 { get; }
    public IRefrigerant Point6 => HeatReleaser.Outlet;
    public IRefrigerant Point7 { get; private set; }
    public IRefrigerant Point8 { get; private set; } = default!;
    public IRefrigerant Point9 { get; private set; } = default!;
    public IRefrigerant Point10 { get; private set; } = default!;
    public IRefrigerant Point11 { get; private set; } = default!;
    public IRefrigerant Point12 { get; private set; } = default!;
    public sealed override Pressure IntermediatePressure => base.IntermediatePressure;
    public sealed override Ratio IntermediateSpecificMassFlow => base.IntermediateSpecificMassFlow;

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow
        * (1 + (Point8.Enthalpy - Point11.Enthalpy) / (Point10.Enthalpy - Point9.Enthalpy));

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point3s.Enthalpy
        - Point2.Enthalpy
        + HeatReleaserSpecificMassFlow.DecimalFractions * (Point5s.Enthalpy - Point4.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point12.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions * (Point5.Enthalpy - Point6.Enthalpy);

    public override IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) => Analyzer.PerformAnalysis(indoor, outdoor);

    private void CalculateInjectionQuality()
    {
        RecuperatorHighPressure = CalculateIntermediatePressure(
            IntermediatePressure,
            HeatReleaser.Pressure
        );
        var validator = new VCRCMitsubishiZubadanValidator();
        do
        {
            Point4 = Refrigerant.DewPointAt(IntermediatePressure);
            Point7 = Point6.IsenthalpicExpansionTo(RecuperatorHighPressure);
            Point8 = Refrigerant.BubblePointAt(RecuperatorHighPressure);
            Point9 = Point8.IsenthalpicExpansionTo(IntermediatePressure);
            Point11 = Point8.CoolingTo(Point9.Temperature + Economizer.TemperatureDifference);
            Point12 = Point11.IsenthalpicExpansionTo(Evaporator.Pressure);
            try
            {
                NewtonRaphson.FindRootNearGuess(
                    ToSolve,
                    Differentiate.FirstDerivativeFunc(ToSolve),
                    80,
                    1e-9,
                    100 - 1e-9,
                    1e-3
                );
            }
            catch (Exception)
            {
                throw new ArgumentException("Solution not found!");
            }

            if (!validator.Validate(this).IsValid)
            {
                RecuperatorHighPressure = CalculateIntermediatePressure(
                    RecuperatorHighPressure,
                    Condenser.Pressure
                );
            }
        } while (!validator.Validate(this).IsValid);

        return;

        double ToSolve(double injectionQuality)
        {
            Point10 = Refrigerant.TwoPhasePointAt(IntermediatePressure, injectionQuality.Percent());
            Point2 = Point1.HeatingTo(
                Point1.Enthalpy
                    + HeatReleaserSpecificMassFlow
                        / EvaporatorSpecificMassFlow
                        * (Point7.Enthalpy - Point8.Enthalpy)
            );
            Point3s = Point2.IsentropicCompressionTo(IntermediatePressure);
            Point3 = Point2.CompressionTo(IntermediatePressure, Compressor.Efficiency);
            return (
                Point10.Enthalpy
                - (
                    Point4.Enthalpy
                    - EvaporatorSpecificMassFlow
                        / IntermediateSpecificMassFlow
                        * (Point3.Enthalpy - Point4.Enthalpy)
                )
            ).JoulesPerKilogram;
        }
    }
}
