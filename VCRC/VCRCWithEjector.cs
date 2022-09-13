using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using UnitsNet;

namespace VCRC;

/// <summary>
///     Single-stage VCRC with an ejector as an expansion device.
/// </summary>
public class VCRCWithEjector : AbstractVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Single-stage VCRC with an ejector as an expansion device.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <param name="ejector">Ejector.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Refrigerant should be a single component or an azeotropic blend!
    /// </exception>
    public VCRCWithEjector(Evaporator evaporator, Compressor compressor, IHeatReleaser heatReleaser,
        Ejector ejector) : base(evaporator, compressor, heatReleaser)
    {
        new RefrigerantTypeValidator().ValidateAndThrow(Refrigerant);
        Ejector = ejector;
        EjectorFlows = Ejector.CalculateFlows(Point3, Point9);
        Point1 = Refrigerant.DewPointAt(Point6.Pressure);
        Point2s = Point1.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point2 = Point1.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
        Point7 = Refrigerant.BubblePointAt(Point6.Pressure);
        Point8 = Point7.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    private EjectorFlows EjectorFlows { get; }

    /// <summary>
    ///     Ejector as a VCRC component.
    /// </summary>
    public Ejector Ejector { get; }

    /// <summary>
    ///     Point 1 - separator vapor outlet / compression stage suction.
    /// </summary>
    public Refrigerant Point1 { get; }

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
    ///     Point 3 – condenser or gas cooler outlet / ejector nozzle inlet.
    /// </summary>
    public Refrigerant Point3 => HeatReleaser.Outlet;

    /// <summary>
    ///     Point 4 – ejector nozzle outlet.
    /// </summary>
    public Refrigerant Point4 => EjectorFlows.NozzleOutlet;

    /// <summary>
    ///     Point 5 – ejector mixing section inlet.
    /// </summary>
    public Refrigerant Point5 => EjectorFlows.MixingInlet;

    /// <summary>
    ///     Point 6 – ejector diffuser outlet / separator inlet.
    /// </summary>
    public Refrigerant Point6 => EjectorFlows.DiffuserOutlet;

    /// <summary>
    ///     Point 7 – separator liquid outlet / EV inlet.
    /// </summary>
    public Refrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – evaporator outlet / ejector suction section inlet.
    /// </summary>
    public Refrigerant Point9 => Evaporator.Outlet;

    /// <summary>
    ///     Point 10 – ejector suction section outlet.
    /// </summary>
    public Refrigerant Point10 => EjectorFlows.SuctionOutlet;

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow *
        (Point6.Quality!.Value.DecimalFractions /
         (1 - Point6.Quality!.Value.DecimalFractions));

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        HeatReleaserSpecificMassFlow.DecimalFractions *
        (Point2s.Enthalpy - Point1.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point9.Enthalpy - Point8.Enthalpy;

    public override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions *
        (Point2.Enthalpy - Point3.Enthalpy);

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor) =>
        new EntropyAnalyzer(this, indoor, outdoor,
                new EvaporatorInfo(EvaporatorSpecificMassFlow, Point8, Point9),
                new HeatReleaserInfo(HeatReleaserSpecificMassFlow, Point2s, Point3),
                new EVInfo(EvaporatorSpecificMassFlow, Point7, Point8), null, null,
                new EjectorInfo(Point6, HeatReleaserSpecificMassFlow, Point3,
                    EvaporatorSpecificMassFlow, Point9))
            .Result;
}