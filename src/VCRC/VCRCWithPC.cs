﻿// ReSharper disable InconsistentNaming

namespace VCRC;

/// <inheritdoc cref="IVCRCWithPC"/>
public class VCRCWithPC : AbstractTwoStageVCRC, IVCRCWithPC
{
    /// <inheritdoc cref="VCRCWithPC"/>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Refrigerant should be a single component or an azeotropic blend!
    /// </exception>
    public VCRCWithPC(IEvaporator evaporator, ICompressor compressor, IHeatReleaser heatReleaser)
        : base(evaporator, compressor, heatReleaser)
    {
        new RefrigerantTypeValidator().ValidateAndThrow(Refrigerant);
        Point2s = Point1.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point2 = Point1.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
        Point3 = Refrigerant.DewPointAt(IntermediatePressure);
        Point7 = Point6.IsenthalpicExpansionTo(IntermediatePressure);
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point4 = Point3.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
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
        Point8 = Refrigerant.BubblePointAt(IntermediatePressure);
        Point9 = Point8.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    private IRefrigerant Point5s { get; }

    private IEntropyAnalyzer Analyzer =>
        new EntropyAnalyzer(
            this,
            new EvaporatorNode(EvaporatorSpecificMassFlow, Point9, Point1),
            new HeatReleaserNode(HeatReleaserSpecificMassFlow, Point5s, Point6),
            new EVNode(HeatReleaserSpecificMassFlow, Point6, Point7),
            new EVNode(EvaporatorSpecificMassFlow, Point8, Point9),
            null,
            null,
            null,
            null,
            new MixingNode(
                Point5,
                EvaporatorSpecificMassFlow,
                Point2,
                IntermediateSpecificMassFlow,
                Point4
            )
        );

    public IRefrigerant Point1 => Evaporator.Outlet;
    public IRefrigerant Point2s { get; }
    public IRefrigerant Point2 { get; }
    public IRefrigerant Point3 { get; }
    public IRefrigerant Point4s { get; }
    public IRefrigerant Point4 { get; }
    public IRefrigerant Point5 { get; }
    public IRefrigerant Point6 => HeatReleaser.Outlet;
    public IRefrigerant Point7 { get; }
    public IRefrigerant Point8 { get; }
    public IRefrigerant Point9 { get; }
    public sealed override Pressure IntermediatePressure => base.IntermediatePressure;
    public sealed override Ratio IntermediateSpecificMassFlow => base.IntermediateSpecificMassFlow;

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow
        * (
            1
            + Point7.Quality!.Value.DecimalFractions / (1 - Point7.Quality!.Value.DecimalFractions)
        );

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy
        - Point1.Enthalpy
        + IntermediateSpecificMassFlow.DecimalFractions * (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point9.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions * (Point5.Enthalpy - Point6.Enthalpy);

    public override IEntropyAnalysisResult EntropyAnalysis(
        Temperature indoor,
        Temperature outdoor
    ) => Analyzer.PerformAnalysis(indoor, outdoor);
}
