using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;

namespace VCRC;

/// <summary>
///     Single-stage VCRC with recuperator.
/// </summary>
public class VCRCWithRecuperator : AbstractVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Single-stage VCRC with recuperator.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="recuperator">Recuperator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at recuperator 'hot' side!
    /// </exception>
    public VCRCWithRecuperator(Evaporator evaporator, Recuperator recuperator, Compressor compressor,
        IHeatReleaser heatReleaser) : base(evaporator, compressor, heatReleaser)
    {
        Recuperator = recuperator;
        new VCRCWithRecuperatorValidator().ValidateAndThrow(this);
        Point2 = Point1.HeatingTo(Point4.Temperature - Recuperator.TemperatureDifference);
        Point3s = Point2.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point3 = Point2.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
        Point5 = Point4.CoolingTo(Point4.Enthalpy - (Point2.Enthalpy - Point1.Enthalpy));
        Point6 = Point5.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    /// <summary>
    ///     Recuperator as a VCRC component.
    /// </summary>
    public Recuperator Recuperator { get; }

    /// <summary>
    ///     Point 1 – evaporator outlet / recuperator "cold" inlet.
    /// </summary>
    public Refrigerant Point1 => Evaporator.Outlet;

    /// <summary>
    ///     Point 2 – recuperator "cold" outlet / compression stage suction.
    /// </summary>
    public Refrigerant Point2 { get; }

    /// <summary>
    ///     Point 3s – isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point3s { get; }

    /// <summary>
    ///     Point 3 – compression stage discharge / condenser or gas cooler inlet.
    /// </summary>
    public Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4 – condenser or gas cooler outlet / recuperator "hot" inlet.
    /// </summary>
    public Refrigerant Point4 => HeatReleaser.Outlet;

    /// <summary>
    ///     Point 5 – recuperator "hot" outlet / EV inlet.
    /// </summary>
    public Refrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point6 { get; }

    public sealed override Ratio HeatReleaserSpecificMassFlow { get; } = 100.Percent();

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point3s.Enthalpy - Point2.Enthalpy;

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point6.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        Point3.Enthalpy - Point4.Enthalpy;

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor) =>
        new EntropyAnalyzer(
                this, indoor, outdoor,
                new EvaporatorInfo(EvaporatorSpecificMassFlow, Point6, Point1),
                new HeatReleaserInfo(HeatReleaser, HeatReleaserSpecificMassFlow, Point3s, Point4),
                new EVInfo(HeatReleaserSpecificMassFlow, Point5, Point6), null, null,
                new RecuperatorInfo(EvaporatorSpecificMassFlow, Point1, Point2,
                    HeatReleaserSpecificMassFlow, Point4, Point5))
            .Result;
}