using System;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.NumberExtensions.NumberToSpeed;

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
    public VCRCWithEjector(Evaporator evaporator, Compressor compressor, IHeatReleaser heatReleaser,
        Ejector ejector) : base(evaporator, compressor, heatReleaser)
    {
        Ejector = ejector;
        Point4 = Point3.ExpansionTo(EjectorMixingPressure, Ejector.NozzleEfficiency);
        Point10 = Point9.ExpansionTo(EjectorMixingPressure, Ejector.SuctionEfficiency);
        CalculateEjectorFlowRatio(); // Also calculates Point5 and Point6
        Point1 = Refrigerant.DewPointAt(EjectorDiffuserPressure);
        Point2s = Point1.IsentropicCompressionTo(HeatReleaser.Pressure);
        Point2 = Point1.CompressionTo(HeatReleaser.Pressure, Compressor.Efficiency);
        Point7 = Refrigerant.BubblePointAt(EjectorDiffuserPressure);
        Point8 = Point7.IsenthalpicExpansionTo(Evaporator.Pressure);
    }

    private Speed Point5Speed =>
        EjectorFlowRatio.DecimalFractions *
        CalculateOutletSpeed(Point3, Point4) +
        (1 - EjectorFlowRatio.DecimalFractions) *
        CalculateOutletSpeed(Point9, Point10);

    private SpecificEnergy Point5KineticEnergy =>
        (Math.Pow(Point5Speed.MetersPerSecond, 2) / 2.0)
        .JoulesPerKilogram();

    private Ratio EjectorFlowRatio { get; set; }

    /// <summary>
    ///     Ejector as a VCRC component.
    /// </summary>
    public Ejector Ejector { get; }

    /// <summary>
    ///     Ejector mixing section pressure.
    /// </summary>
    public Pressure EjectorMixingPressure => 0.9 * Evaporator.Pressure;

    /// <summary>
    ///     Ejector diffuser outlet pressure.
    /// </summary>
    public Pressure EjectorDiffuserPressure =>
        Refrigerant.WithState(Input.Entropy(Point5.Entropy),
                Input.Enthalpy(
                    Point5.Enthalpy +
                    Ejector.DiffuserEfficiency.DecimalFractions * Point5KineticEnergy))
            .Pressure;

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
    public Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – ejector diffuser inlet.
    /// </summary>
    public Refrigerant Point5 { get; private set; } = null!;

    /// <summary>
    ///     Point 6 – ejector diffuser outlet / separator inlet.
    /// </summary>
    public Refrigerant Point6 { get; private set; } = null!;

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
    public Refrigerant Point10 { get; }

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

    private void CalculateEjectorFlowRatio()
    {
        double ToSolve(double ejectorFlowRatio)
        {
            EjectorFlowRatio = ejectorFlowRatio.Percent();
            Point5 = Refrigerant.WithState(Input.Pressure(EjectorMixingPressure),
                Input.Enthalpy(
                    EjectorFlowRatio.DecimalFractions * Point3.Enthalpy +
                    (1 - EjectorFlowRatio.DecimalFractions) * Point9.Enthalpy -
                    Point5KineticEnergy));
            Point6 = Refrigerant.WithState(Input.Pressure(EjectorDiffuserPressure),
                Input.Enthalpy(Point5.Enthalpy + Point5KineticEnergy));
            return (Point6.Quality!.Value - EjectorFlowRatio).Percent;
        }

        NewtonRaphson.FindRootNearGuess(
            ToSolve, Differentiate.FirstDerivativeFunc(ToSolve), 50, 1e-9, 100 - 1e-9, 1e-6);
    }

    private static Speed CalculateOutletSpeed(AbstractFluid inlet, AbstractFluid outlet) =>
        Math.Sqrt(2 * (inlet.Enthalpy - outlet.Enthalpy).JoulesPerKilogram).MetersPerSecond();
}