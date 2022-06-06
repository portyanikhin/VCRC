using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using UnitsNet;

namespace VCRC;

/// <summary>
///     Two-stage VCRC with incomplete intercooling.
/// </summary>
public class VCRCWithIIC : AbstractTwoStageVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Two-stage VCRC with incomplete intercooling.
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
    public VCRCWithIIC(Evaporator evaporator, Compressor compressor, IHeatReleaser heatReleaser) :
        base(evaporator, compressor, heatReleaser)
    {
        new RefrigerantWithoutGlideValidator().ValidateAndThrow(Refrigerant);
        Point2s = Point1.IsentropicCompressionTo(IntermediatePressure);
        Point2 = Point1.CompressionTo(IntermediatePressure, Compressor.Efficiency);
        Point6 = Point5.IsenthalpicExpansionTo(IntermediatePressure);
        Point7 = Refrigerant.DewPointAt(IntermediatePressure);
        Point8 = Refrigerant.BubblePointAt(IntermediatePressure);
        Point9 = Point8.IsenthalpicExpansionTo(Evaporator.Pressure);
        Point3 = Refrigerant.Mixing(EvaporatorSpecificMassFlow, Point2,
            IntermediateSpecificMassFlow, Point7);
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point4 = Point3.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
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
    ///     Point 3 – second compression stage suction.
    /// </summary>
    public Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point4s { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge / condenser or gas cooler inlet.
    /// </summary>
    public Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – condenser or gas cooler outlet / first EV inlet.
    /// </summary>
    public Refrigerant Point5 => HeatReleaser.Outlet;

    /// <summary>
    ///     Point 6 – first EV outlet / separator inlet.
    /// </summary>
    public Refrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – separator vapor outlet / injection of cooled vapor into the compressor.
    /// </summary>
    public Refrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – separator liquid outlet / second EV inlet.
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
        EvaporatorSpecificMassFlow /
        (1 - Point6.Quality!.Value.DecimalFractions);

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy - Point1.Enthalpy +
        HeatReleaserSpecificMassFlow.DecimalFractions *
        (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point9.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions *
        (Point4.Enthalpy - Point5.Enthalpy);

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor) =>
        new EntropyAnalyzer(
                this, indoor, outdoor,
                new EvaporatorInfo(EvaporatorSpecificMassFlow, Point9, Point1),
                new HeatReleaserInfo(HeatReleaserSpecificMassFlow, Point4s, Point5),
                new EVInfo(HeatReleaserSpecificMassFlow, Point5, Point6),
                new EVInfo(EvaporatorSpecificMassFlow, Point8, Point9), null, null, null, null,
                new MixingInfo(Point3, EvaporatorSpecificMassFlow, Point2,
                    IntermediateSpecificMassFlow, Point7))
            .Result;
}