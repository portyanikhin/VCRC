using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using UnitsNet;

namespace VCRC;

/// <summary>
///     Two-stage VCRC with parallel compression.
/// </summary>
public class VCRCWithParallelCompression : AbstractTwoStageVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Two-stage VCRC with parallel compression.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Refrigerant should not have a temperature glide!
    /// </exception>
    public VCRCWithParallelCompression(Evaporator evaporator, Compressor compressor,
        IHeatReleaser heatReleaser) : base(evaporator, compressor, heatReleaser)
    {
        new RefrigerantWithoutGlideValidator().ValidateAndThrow(Refrigerant);
        Point2s = Point1.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point2 = Point1.CompressionTo(HeatReleaser.Pressure, Compressor.IsentropicEfficiency);
        Point3 = Refrigerant.DewPointAt(IntermediatePressure);
        Point7 = Point6.IsenthalpicExpansionTo(IntermediatePressure);
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point4 = Point3.CompressionTo(HeatReleaser.Pressure, Compressor.IsentropicEfficiency);
        Point5s = Refrigerant.Mixing(EvaporatorSpecificMassFlow, Point2s,
            HeatReleaserSpecificMassFlow - EvaporatorSpecificMassFlow, Point4s);
        Point5 = Refrigerant.Mixing(EvaporatorSpecificMassFlow, Point2,
            HeatReleaserSpecificMassFlow - EvaporatorSpecificMassFlow, Point4);
        Point8 = Refrigerant.BubblePointAt(IntermediatePressure);
        Point9 = Point8.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

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
    ///     Point 3 – intermediate vessel vapor outlet / second compression stage suction.
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
    ///     Point 6 – condenser or gas cooler outlet / first EV inlet.
    /// </summary>
    public Refrigerant Point6 => HeatReleaser.Outlet;

    /// <summary>
    ///     Point 7 – first EV outlet / intermediate vessel inlet.
    /// </summary>
    public Refrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – intermediate vessel liquid outlet / second EV inlet.
    /// </summary>
    public Refrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – second EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point9 { get; }

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow *
        (1 + Point7.Quality!.Value.DecimalFractions /
            (1 - Point7.Quality!.Value.DecimalFractions));

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy - Point1.Enthalpy +
        (HeatReleaserSpecificMassFlow - EvaporatorSpecificMassFlow).DecimalFractions *
        (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point9.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions * (Point5.Enthalpy - Point6.Enthalpy);

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor) =>
        new EntropyAnalyzer(
                this, indoor, outdoor,
                new EvaporatorInfo(EvaporatorSpecificMassFlow, Point9, Point1),
                new HeatReleaserInfo(HeatReleaser, HeatReleaserSpecificMassFlow, Point5s, Point6),
                new EVInfo(HeatReleaserSpecificMassFlow, Point6, Point7),
                new EVInfo(EvaporatorSpecificMassFlow, Point8, Point9), null, null, null,
                new MixingInfo(Point5, EvaporatorSpecificMassFlow, Point2,
                    HeatReleaserSpecificMassFlow - EvaporatorSpecificMassFlow, Point4))
            .Result;
}