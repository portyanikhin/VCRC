﻿namespace VCRC;

/// <summary>
///     Two-stage VCRC with an ejector as an expansion device and economizer.
/// </summary>
public class VCRCWithEjectorAndEconomizer
    : AbstractTwoStageVCRC,
        IVCRCWithEjectorAndEconomizer
{
    private Pressure _diffuserOutletPressure;
    private IEjectorFlows _ejectorFlows = default!;

    /// <summary>
    ///     Two-stage VCRC with an ejector as an expansion device and economizer.
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
    public VCRCWithEjectorAndEconomizer(
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
            _ejectorFlows.DiffuserOutlet.Pressure
        );
        Point13 = Point12.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point13, Point14),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point4s, Point5),
            new EVNode(IntermediateSpecificMassFlow, Point5, Point6),
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
                Point6,
                Point7,
                HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
                Point5,
                Point8
            ),
            new MixingNode(
                Point3,
                HeatReleaserSpecificMassFlow - IntermediateSpecificMassFlow,
                Point2,
                IntermediateSpecificMassFlow,
                Point7
            )
        );

    public IEjector Ejector { get; }

    public IEconomizer Economizer { get; }

    public IRefrigerant Point1 { get; }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point2s { get; }

    public IRefrigerant Point2 { get; }

    public IRefrigerant Point3 { get; }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point4s { get; }

    public IRefrigerant Point4 { get; }

    public IRefrigerant Point5 => HeatReleaser.Outlet;

    public IRefrigerant Point6 { get; private set; } = default!;

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
            Point6 = Point5.IsenthalpicExpansionTo(IntermediatePressure);
            Point7 = Refrigerant.Superheated(
                IntermediatePressure,
                Economizer.Superheat
            );
            new VCRCWithEjectorAndEconomizerValidator().ValidateAndThrow(this);
            Point8 = Point5.CoolingTo(
                Point6.Temperature + Economizer.TemperatureDifference
            );
            _ejectorFlows = Ejector.CalculateFlows(Point8, Point14);
            return (
                _ejectorFlows.DiffuserOutlet.Pressure - _diffuserOutletPressure
            ).Pascals;
        }
    }
}
