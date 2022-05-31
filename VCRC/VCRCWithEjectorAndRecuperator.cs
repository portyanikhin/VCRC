using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;

namespace VCRC;

/// <summary>
///     Single-stage VCRC with an ejector as an expansion device and recuperator.
/// </summary>
public class VCRCWithEjectorAndRecuperator : AbstractVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Single-stage VCRC with an ejector as an expansion device and recuperator.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="recuperator">Recuperator.</param>
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
    ///     Refrigerant should not have a temperature glide!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at recuperator 'hot' side!
    /// </exception>
    public VCRCWithEjectorAndRecuperator(Evaporator evaporator, Recuperator recuperator,
        Compressor compressor, IHeatReleaser heatReleaser, Ejector ejector) :
        base(evaporator, compressor, heatReleaser)
    {
        new RefrigerantWithoutGlideValidator().ValidateAndThrow(Refrigerant);
        (Recuperator, Ejector) = (recuperator, ejector);
        CalculateDiffuserOutletPressure();
        Point9 = Refrigerant.BubblePointAt(Point8.Pressure);
        Point10 = Point9.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    private EjectorFlows EjectorFlows { get; set; } = null!;

    /// <summary>
    ///     Recuperator as a VCRC component.
    /// </summary>
    public Recuperator Recuperator { get; }

    /// <summary>
    ///     Ejector as a VCRC component.
    /// </summary>
    public Ejector Ejector { get; }

    /// <summary>
    ///     Point 1 - separator vapor outlet / recuperator "cold" inlet.
    /// </summary>
    public Refrigerant Point1 { get; private set; } = null!;

    /// <summary>
    ///     Point 2 - recuperator "cold" outlet / compression stage suction.
    /// </summary>
    public Refrigerant Point2 { get; private set; } = null!;

    /// <summary>
    ///     Point 3s – isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point3s { get; private set; } = null!;

    /// <summary>
    ///     Point 3 - compression stage discharge / condenser or gas cooler inlet.
    /// </summary>
    public Refrigerant Point3 { get; private set; } = null!;

    /// <summary>
    ///     Point 4 – condenser or gas cooler outlet / recuperator "hot" inlet.
    /// </summary>
    public Refrigerant Point4 => HeatReleaser.Outlet;

    /// <summary>
    ///     Point 5 – recuperator "hot" outlet / ejector nozzle inlet.
    /// </summary>
    public Refrigerant Point5 { get; private set; } = null!;

    /// <summary>
    ///     Point 6 – ejector nozzle outlet.
    /// </summary>
    public Refrigerant Point6 => EjectorFlows.NozzleOutlet;

    /// <summary>
    ///     Point 7 – ejector mixing section inlet.
    /// </summary>
    public Refrigerant Point7 => EjectorFlows.MixingInlet;

    /// <summary>
    ///     Point 8 – ejector diffuser outlet / separator inlet.
    /// </summary>
    public Refrigerant Point8 => EjectorFlows.DiffuserOutlet;

    /// <summary>
    ///     Point 9 – separator liquid outlet / EV inlet.
    /// </summary>
    public Refrigerant Point9 { get; }

    /// <summary>
    ///     Point 10 – EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point10 { get; }

    /// <summary>
    ///     Point 11 – evaporator outlet / ejector suction section inlet.
    /// </summary>
    public Refrigerant Point11 => Evaporator.Outlet;

    /// <summary>
    ///     Point 12 – ejector suction section outlet.
    /// </summary>
    public Refrigerant Point12 => EjectorFlows.SuctionOutlet;

    public override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow *
        (Point8.Quality!.Value.DecimalFractions /
         (1 - Point8.Quality!.Value.DecimalFractions));

    public override SpecificEnergy IsentropicSpecificWork =>
        HeatReleaserSpecificMassFlow.DecimalFractions *
        (Point3s.Enthalpy - Point2.Enthalpy);

    public override SpecificEnergy SpecificCoolingCapacity =>
        Point11.Enthalpy - Point10.Enthalpy;

    public override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions *
        (Point3.Enthalpy - Point4.Enthalpy);

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor) =>
        new EntropyAnalyzer(this, indoor, outdoor,
                new EvaporatorInfo(EvaporatorSpecificMassFlow, Point10, Point11),
                new HeatReleaserInfo(HeatReleaserSpecificMassFlow, Point3s, Point4),
                new EVInfo(EvaporatorSpecificMassFlow, Point9, Point10), null, null,
                new EjectorInfo(Point8, HeatReleaserSpecificMassFlow, Point5,
                    EvaporatorSpecificMassFlow, Point11),
                new RecuperatorInfo(HeatReleaserSpecificMassFlow, Point1, Point2,
                    HeatReleaserSpecificMassFlow, Point4, Point5))
            .Result;

    private void CalculateDiffuserOutletPressure()
    {
        double ToSolve(double diffuserOutletPressure)
        {
            Point1 = Refrigerant.DewPointAt(diffuserOutletPressure.Pascals());
            new VCRCWithEjectorAndRecuperatorValidator().ValidateAndThrow(this);
            Point2 = Point1.HeatingTo(Point4.Temperature - Recuperator.TemperatureDifference);
            Point3s = Point2.IsentropicCompressionTo(HeatReleaser.Pressure);
            Point3 = Point2.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
            Point5 = Point4.CoolingTo(Point4.Enthalpy - (Point2.Enthalpy - Point1.Enthalpy));
            EjectorFlows = Ejector.CalculateFlows(Point5, Point11);
            return (EjectorFlows.DiffuserOutlet.Pressure - Point1.Pressure).Pascals;
        }

        NewtonRaphson.FindRootNearGuess(
            ToSolve, Differentiate.FirstDerivativeFunc(ToSolve), Evaporator.Pressure.Pascals + 100,
            Evaporator.Pressure.Pascals + 1, HeatReleaser.Pressure.Pascals - 1, 10);
    }
}