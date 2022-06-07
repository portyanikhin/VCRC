using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;

namespace VCRC;

/// <summary>
///     Two-stage VCRC with economizer and two-phase injection into the compressor.
/// </summary>
public class VCRCWithEconomizerAndTPI : AbstractTwoStageVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Two-stage VCRC with economizer and two-phase injection into the compressor.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at economizer 'cold' side!
    /// </exception>
    public VCRCWithEconomizerAndTPI(Evaporator evaporator, Compressor compressor, IHeatReleaser heatReleaser,
        EconomizerWithTPI economizer) : base(evaporator, compressor, heatReleaser)
    {
        Economizer = economizer;
        Point2s = Point1.IsentropicCompressionTo(IntermediatePressure);
        Point2 = Point1.CompressionTo(IntermediatePressure, Compressor.Efficiency);
        Point3 = Refrigerant.DewPointAt(IntermediatePressure);
        Point4s = Point3.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point4 = Point3.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
        Point6 = Point5.IsenthalpicExpansionTo(IntermediatePressure);
        new VCRCWithEconomizerAndTPIValidator().ValidateAndThrow(this);
        Point8 = Point5.CoolingTo(Point6.Temperature + Economizer.TemperatureDifference);
        Point7 = Point6.HeatingTo(
            ((Point6.Enthalpy.JoulesPerKilogram *
              (Point2.Enthalpy.JoulesPerKilogram - Point3.Enthalpy.JoulesPerKilogram) +
              Point3.Enthalpy.JoulesPerKilogram *
              (Point5.Enthalpy.JoulesPerKilogram - Point8.Enthalpy.JoulesPerKilogram)) /
             (Point2.Enthalpy.JoulesPerKilogram - Point3.Enthalpy.JoulesPerKilogram +
                 Point5.Enthalpy.JoulesPerKilogram - Point8.Enthalpy.JoulesPerKilogram))
            .JoulesPerKilogram());
        Point9 = Point8.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    /// <summary>
    ///     Economizer as a VCRC component.
    /// </summary>
    public EconomizerWithTPI Economizer { get; }

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
    ///     Point 5 – condenser or gas cooler outlet / first EV inlet / economizer "hot" inlet.
    /// </summary>
    public Refrigerant Point5 => HeatReleaser.Outlet;

    /// <summary>
    ///     Point 6 – first EV outlet / economizer "cold" inlet.
    /// </summary>
    public Refrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
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
        (1 + (Point2.Enthalpy - Point3.Enthalpy) /
            (Point3.Enthalpy - Point7.Enthalpy));

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
                new EVInfo(IntermediateSpecificMassFlow, Point5, Point6),
                new EVInfo(EvaporatorSpecificMassFlow, Point8, Point9), null, null, null,
                new EconomizerInfo(IntermediateSpecificMassFlow, Point6, Point7,
                    EvaporatorSpecificMassFlow, Point5, Point8),
                new MixingInfo(Point3, EvaporatorSpecificMassFlow, Point2,
                    IntermediateSpecificMassFlow, Point7))
            .Result;
}