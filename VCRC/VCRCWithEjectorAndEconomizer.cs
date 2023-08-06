namespace VCRC;

/// <summary>
///     Two-stage VCRC with an ejector
///     as an expansion device and economizer.
/// </summary>
public class VCRCWithEjectorAndEconomizer
    : AbstractTwoStageVCRC,
        IEntropyAnalysable
{
    /// <summary>
    ///     Two-stage VCRC with an ejector
    ///     as an expansion device and economizer.
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
    ///     Wrong temperature difference at economizer 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at economizer 'cold' side!
    /// </exception>
    public VCRCWithEjectorAndEconomizer(
        Evaporator evaporator,
        Compressor compressor,
        IHeatReleaser heatReleaser,
        Ejector ejector,
        Economizer economizer
    )
        : base(evaporator, compressor, heatReleaser)
    {
        new RefrigerantTypeValidator().ValidateAndThrow(Refrigerant);
        (Ejector, Economizer) = (ejector, economizer);
        CalculateDiffuserOutletPressure();
        Point1 = Refrigerant.DewPointAt(EjectorFlows.DiffuserOutlet.Pressure);
        Point2s = Point1.IsentropicCompressionTo(IntermediatePressure);
        Point2 = Point1.CompressionTo(
            IntermediatePressure,
            Compressor.Efficiency
        );
        Point3 = Refrigerant.Mixing(
            HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
            Point2,
            IntermediateSpecificMassFlow,
            Point7
        );
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point4 = Point3.CompressionTo(
            HeatReleaser.Pressure,
            Compressor.Efficiency
        );
        Point12 = Refrigerant.BubblePointAt(
            EjectorFlows.DiffuserOutlet.Pressure
        );
        Point13 = Point12.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    private EjectorFlows EjectorFlows { get; set; } = null!;

    private Pressure DiffuserOutletPressure { get; set; }

    /// <summary>
    ///     Ejector as a VCRC component.
    /// </summary>
    public Ejector Ejector { get; }

    /// <summary>
    ///     Economizer as a VCRC component.
    /// </summary>
    public Economizer Economizer { get; }

    /// <summary>
    ///     Point 1 - separator vapor outlet / first compression stage suction.
    /// </summary>
    public Refrigerant Point1 { get; }

    /// <summary>
    ///     Point 2s – first isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – first compression stage discharge.
    /// </summary>
    public Refrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – second compression stage suction.
    /// </summary>
    public Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point4s { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge /
    ///     condenser or gas cooler inlet.
    /// </summary>
    public Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – condenser or gas cooler outlet /
    ///     first EV inlet / economizer "hot" inlet.
    /// </summary>
    public Refrigerant Point5 => HeatReleaser.Outlet;

    /// <summary>
    ///     Point 6 – first EV outlet / economizer "cold" inlet.
    /// </summary>
    public Refrigerant Point6 { get; private set; } = null!;

    /// <summary>
    ///     Point 7 – economizer "cold" outlet /
    ///     injection of cooled vapor into the compressor.
    /// </summary>
    public Refrigerant Point7 { get; private set; } = null!;

    /// <summary>
    ///     Point 8 – economizer "hot" outlet / ejector nozzle inlet.
    /// </summary>
    public Refrigerant Point8 { get; private set; } = null!;

    /// <summary>
    ///     Point 9 – ejector nozzle outlet.
    /// </summary>
    public Refrigerant Point9 => EjectorFlows.NozzleOutlet;

    /// <summary>
    ///     Point 10 – ejector mixing section inlet.
    /// </summary>
    public Refrigerant Point10 => EjectorFlows.MixingInlet;

    /// <summary>
    ///     Point 11 – ejector diffuser outlet / separator inlet.
    /// </summary>
    public Refrigerant Point11 => EjectorFlows.DiffuserOutlet;

    /// <summary>
    ///     Point 12 – separator liquid outlet / second EV inlet.
    /// </summary>
    public Refrigerant Point12 { get; }

    /// <summary>
    ///     Point 13 – second EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point13 { get; }

    /// <summary>
    ///     Point 14 – evaporator outlet / ejector suction section inlet.
    /// </summary>
    public Refrigerant Point14 => Evaporator.Outlet;

    /// <summary>
    ///     Point 15 – ejector suction section outlet.
    /// </summary>
    public Refrigerant Point15 => EjectorFlows.SuctionOutlet;

    public sealed override Pressure IntermediatePressure =>
        CalculateIntermediatePressure(
            DiffuserOutletPressure,
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
            + (Point5.Enthalpy - Point8.Enthalpy)
                / (Point7.Enthalpy - Point6.Enthalpy)
        );

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        (
            HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow
        ).DecimalFractions * (Point2s.Enthalpy - Point1.Enthalpy)
        + HeatReleaserSpecificMassFlow.DecimalFractions
            * (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point14.Enthalpy - Point13.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions
        * (Point4.Enthalpy - Point5.Enthalpy);

    public EntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) =>
        new EntropyAnalyzer(
            this,
            indoor,
            outdoor,
            new EvaporatorInfo(EvaporatorSpecificMassFlow, Point13, Point14),
            new HeatReleaserInfo(HeatReleaserSpecificMassFlow, Point4s, Point5),
            new EVInfo(IntermediateSpecificMassFlow, Point5, Point6),
            new EVInfo(EvaporatorSpecificMassFlow, Point12, Point13),
            null,
            new EjectorInfo(
                Point11,
                HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
                Point8,
                EvaporatorSpecificMassFlow,
                Point14
            ),
            null,
            new EconomizerInfo(
                IntermediateSpecificMassFlow,
                Point6,
                Point7,
                HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
                Point5,
                Point8
            ),
            new MixingInfo(
                Point3,
                HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
                Point2,
                IntermediateSpecificMassFlow,
                Point7
            )
        ).Result;

    private void CalculateDiffuserOutletPressure()
    {
        double ToSolve(double diffuserOutletPressure)
        {
            DiffuserOutletPressure = diffuserOutletPressure.Pascals();
            Point6 = Point5.IsenthalpicExpansionTo(IntermediatePressure);
            Point7 = Refrigerant.Superheated(
                IntermediatePressure,
                Economizer.Superheat
            );
            new VCRCWithEjectorAndEconomizerValidator().ValidateAndThrow(this);
            Point8 = Point5.CoolingTo(
                Point6.Temperature + Economizer.TemperatureDifference
            );
            EjectorFlows = Ejector.CalculateFlows(Point8, Point14);
            return (
                EjectorFlows.DiffuserOutlet.Pressure - DiffuserOutletPressure
            ).Pascals;
        }

        NewtonRaphson.FindRootNearGuess(
            ToSolve,
            Differentiate.FirstDerivativeFunc(ToSolve),
            Evaporator.Pressure.Pascals + 100,
            Evaporator.Pressure.Pascals + 1,
            HeatReleaser.Pressure.Pascals - 1,
            10
        );
    }
}
