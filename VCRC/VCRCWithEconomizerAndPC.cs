namespace VCRC;

/// <summary>
///     Two-stage VCRC with economizer and parallel compression.
/// </summary>
public class VCRCWithEconomizerAndPC : AbstractTwoStageVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Two-stage VCRC with economizer and parallel compression.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">Wrong temperature difference at economizer 'hot' side!</exception>
    /// <exception cref="ValidationException">Too high temperature difference at economizer 'cold' side!</exception>
    public VCRCWithEconomizerAndPC(Evaporator evaporator, Compressor compressor,
        IHeatReleaser heatReleaser, Economizer economizer) : base(evaporator, compressor, heatReleaser)
    {
        Economizer = economizer;
        Point2s = Point1.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point2 = Point1.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
        Point7 = Point6.IsenthalpicExpansionTo(IntermediatePressure);
        Point3 = Refrigerant.Superheated(IntermediatePressure, Economizer.Superheat);
        new VCRCWithEconomizerAndPCValidator().ValidateAndThrow(this);
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point4 = Point3.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
        Point8 = Point6.CoolingTo(Point7.Temperature + Economizer.TemperatureDifference);
        Point9 = Point8.IsenthalpicExpansionTo(Evaporator.Pressure);
        Point5s = Refrigerant.Mixing(EvaporatorSpecificMassFlow, Point2s,
            IntermediateSpecificMassFlow, Point4s);
        Point5 = Refrigerant.Mixing(EvaporatorSpecificMassFlow, Point2,
            IntermediateSpecificMassFlow, Point4);
    }

    /// <summary>
    ///     Economizer as a VCRC component.
    /// </summary>
    public Economizer Economizer { get; }

    /// <summary>
    ///     Point 1 – evaporator outlet / first compression stage suction.
    /// </summary>
    public Refrigerant Point1 => Evaporator.Outlet;

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
    ///     Point 3 – economizer "cold" outlet / second compression stage suction.
    /// </summary>
    public Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point4s { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge.
    /// </summary>
    public Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5s – condenser or gas cooler inlet (for isentropic compression).
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private Refrigerant Point5s { get; }

    /// <summary>
    ///     Point 5 – condenser or gas cooler inlet.
    /// </summary>
    public Refrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – condenser or gas cooler outlet / first EV inlet / economizer "hot" inlet.
    /// </summary>
    public Refrigerant Point6 => HeatReleaser.Outlet;

    /// <summary>
    ///     Point 7 –  first EV outlet / economizer "cold" inlet.
    /// </summary>
    public Refrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – economizer "hot" outlet / second EV inlet.
    /// </summary>
    public Refrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – second EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point9 { get; }

    public sealed override Pressure IntermediatePressure =>
        base.IntermediatePressure;

    public sealed override Ratio IntermediateSpecificMassFlow =>
        base.IntermediateSpecificMassFlow;

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow *
        (1 + (Point6.Enthalpy - Point8.Enthalpy) /
            (Point3.Enthalpy - Point7.Enthalpy));

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy - Point1.Enthalpy +
        IntermediateSpecificMassFlow.DecimalFractions *
        (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point9.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions *
        (Point5.Enthalpy - Point6.Enthalpy);

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor) =>
        new EntropyAnalyzer(this, indoor, outdoor,
                new EvaporatorInfo(EvaporatorSpecificMassFlow, Point9, Point1),
                new HeatReleaserInfo(HeatReleaserSpecificMassFlow, Point5s, Point6),
                new EVInfo(IntermediateSpecificMassFlow, Point6, Point7),
                new EVInfo(EvaporatorSpecificMassFlow, Point8, Point9), null, null, null,
                new EconomizerInfo(IntermediateSpecificMassFlow, Point7, Point3,
                    EvaporatorSpecificMassFlow, Point6, Point8),
                new MixingInfo(Point5, EvaporatorSpecificMassFlow, Point2,
                    IntermediateSpecificMassFlow, Point4))
            .Result;
}