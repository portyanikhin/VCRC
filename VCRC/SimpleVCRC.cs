using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;

namespace VCRC;

/// <summary>
///     Simple single-stage VCRC.
/// </summary>
public class SimpleVCRC : AbstractVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Simple single-stage VCRC.
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
    public SimpleVCRC(Evaporator evaporator, Compressor compressor, IHeatReleaser heatReleaser) :
        base(evaporator, compressor, heatReleaser)
    {
        Point2s = Point1.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point2 = Point1.CompressionTo(HeatReleaser.Pressure, Compressor.IsentropicEfficiency);
        Point4 = Point3.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    /// <summary>
    ///     Point 1 – evaporator outlet / compression stage suction.
    /// </summary>
    public Refrigerant Point1 => Evaporator.Outlet;

    /// <summary>
    ///     Point 2s – isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – compression stage discharge / condenser or gas cooler inlet.
    /// </summary>
    public Refrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – condenser or gas cooler outlet / EV inlet.
    /// </summary>
    public Refrigerant Point3 => HeatReleaser.Outlet;

    /// <summary>
    ///     Point 4 – EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point4 { get; }

    public sealed override Ratio HeatReleaserSpecificMassFlow { get; } = 100.Percent();

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point2s.Enthalpy - Point1.Enthalpy;

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point4.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        Point2.Enthalpy - Point3.Enthalpy;

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor) =>
        new EntropyAnalyzer(
                this, indoor, outdoor,
                new EvaporatorInfo(EvaporatorSpecificMassFlow, Point4, Point1),
                new HeatReleaserInfo(HeatReleaser, HeatReleaserSpecificMassFlow, Point2s, Point3),
                new EVInfo(HeatReleaserSpecificMassFlow, Point3, Point4))
            .Result;
}