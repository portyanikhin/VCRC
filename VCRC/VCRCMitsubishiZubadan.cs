using System;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;

namespace VCRC;

/// <summary>
///     Mitsubishi Zubadan VCRC (subcritical only).
/// </summary>
/// <remarks>
///     Two-stage subcritical VCRC with economizer, recuperator and
///     two-phase injection into the compressor.
/// </remarks>
public class VCRCMitsubishiZubadan : AbstractTwoStageVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Mitsubishi Zubadan VCRC (subcritical only).
    /// </summary>
    /// <remarks>
    ///     Two-stage subcritical VCRC with economizer, recuperator and
    ///     two-phase injection into the compressor.
    /// </remarks>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="condenser">Condenser.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ArgumentException">
    ///     Solution not found!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     There should be a two-phase refrigerant at the recuperator 'hot' inlet!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at recuperator 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at recuperator 'cold' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at economizer 'cold' side!
    /// </exception>
    public VCRCMitsubishiZubadan(Evaporator evaporator, Compressor compressor,
        Condenser condenser, EconomizerWithTPI economizer) : base(evaporator, compressor, condenser)
    {
        (Condenser, Economizer) = (condenser, economizer);
        CalculateInjectionQuality();
        new VCRCMitsubishiZubadanValidator().ValidateAndThrow(this);
        Point5s = Point4!.IsentropicCompressionTo(Condenser.Pressure);
        Point5 = Point4.CompressionTo(Condenser.Pressure, Compressor.Efficiency);
        Recuperator = new Recuperator(Point7!.Temperature - Point2!.Temperature);
    }

    /// <summary>
    ///     Condenser as a subcritical VCRC component.
    /// </summary>
    public new Condenser Condenser { get; }

    /// <summary>
    ///     Recuperator as a VCRC component.
    /// </summary>
    public Recuperator Recuperator { get; }

    /// <summary>
    ///     Economizer as a VCRC component.
    /// </summary>
    public EconomizerWithTPI Economizer { get; }

    /// <summary>
    ///     Absolute recuperator high pressure.
    /// </summary>
    public Pressure RecuperatorHighPressure { get; private set; }

    /// <summary>
    ///     Point 1 – evaporator outlet / recuperator "cold" inlet.
    /// </summary>
    public Refrigerant Point1 => Evaporator.Outlet;

    /// <summary>
    ///     Point 2 – recuperator "cold" outlet / first compression stage suction.
    /// </summary>
    public Refrigerant Point2 { get; private set; }

    /// <summary>
    ///     Point 3s – first isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point3s { get; private set; } = null!;

    /// <summary>
    ///     Point 3 – first compression stage discharge.
    /// </summary>
    public Refrigerant Point3 { get; private set; } = null!;

    /// <summary>
    ///     Point 4 – second compression stage suction.
    /// </summary>
    public Refrigerant Point4 { get; private set; }

    /// <summary>
    ///     Point 5s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point5s { get; }

    /// <summary>
    ///     Point 5 – second compression stage discharge / condenser inlet.
    /// </summary>
    public Refrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – condenser outlet / first EV inlet.
    /// </summary>
    public Refrigerant Point6 => HeatReleaser.Outlet;

    /// <summary>
    ///     Point 7 – first EV outlet / recuperator "hot" inlet.
    /// </summary>
    public Refrigerant Point7 { get; private set; }

    /// <summary>
    ///     Point 8 – recuperator "hot" outlet / second EV inlet / economizer "hot" inlet.
    /// </summary>
    public Refrigerant Point8 { get; private set; } = null!;

    /// <summary>
    ///     Point 9 – second EV outlet / economizer "cold" inlet.
    /// </summary>
    public Refrigerant Point9 { get; private set; } = null!;

    /// <summary>
    ///     Point 10 – economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
    /// </summary>
    public Refrigerant Point10 { get; private set; } = null!;

    /// <summary>
    ///     Point 11 – economizer "hot" outlet / third EV inlet.
    /// </summary>
    public Refrigerant Point11 { get; private set; } = null!;

    /// <summary>
    ///     Point 12 – third EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point12 { get; private set; } = null!;

    public sealed override Pressure IntermediatePressure =>
        base.IntermediatePressure;

    public sealed override Ratio IntermediateSpecificMassFlow =>
        base.IntermediateSpecificMassFlow;

    public sealed override Ratio HeatReleaserSpecificMassFlow =>
        EvaporatorSpecificMassFlow *
        (1 + (Point8.Enthalpy - Point11.Enthalpy) /
            (Point10.Enthalpy - Point9.Enthalpy));

    public sealed override SpecificEnergy IsentropicSpecificWork =>
        Point3s.Enthalpy - Point2.Enthalpy +
        HeatReleaserSpecificMassFlow.DecimalFractions *
        (Point5s.Enthalpy - Point4.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point12.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        HeatReleaserSpecificMassFlow.DecimalFractions *
        (Point5.Enthalpy - Point6.Enthalpy);

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor) =>
        new EntropyAnalyzer(this, indoor, outdoor,
                new EvaporatorInfo(EvaporatorSpecificMassFlow, Point12, Point1),
                new HeatReleaserInfo(HeatReleaserSpecificMassFlow, Point5s, Point6),
                new EVInfo(HeatReleaserSpecificMassFlow, Point6, Point7),
                new EVInfo(IntermediateSpecificMassFlow, Point8, Point9),
                new EVInfo(EvaporatorSpecificMassFlow, Point11, Point12), null,
                new RecuperatorInfo(EvaporatorSpecificMassFlow, Point1, Point2,
                    HeatReleaserSpecificMassFlow, Point7, Point8),
                new EconomizerInfo(IntermediateSpecificMassFlow, Point9, Point10,
                    EvaporatorSpecificMassFlow, Point8, Point11),
                new MixingInfo(Point4, EvaporatorSpecificMassFlow, Point3,
                    IntermediateSpecificMassFlow, Point10))
            .Result;

    private void CalculateInjectionQuality()
    {
        double ToSolve(double injectionQuality)
        {
            Point10 = Refrigerant.TwoPhasePointAt(IntermediatePressure, injectionQuality.Percent());
            Point2 = Point1.HeatingTo(
                Point1.Enthalpy +
                HeatReleaserSpecificMassFlow / EvaporatorSpecificMassFlow *
                (Point7.Enthalpy - Point8.Enthalpy));
            Point3s = Point2.IsentropicCompressionTo(IntermediatePressure);
            Point3 = Point2.CompressionTo(IntermediatePressure, Compressor.Efficiency);
            return (Point10.Enthalpy -
                    (Point4.Enthalpy - EvaporatorSpecificMassFlow /
                        IntermediateSpecificMassFlow *
                        (Point3.Enthalpy - Point4.Enthalpy)))
                .JoulesPerKilogram;
        }

        RecuperatorHighPressure = CalculateIntermediatePressure(
            IntermediatePressure, HeatReleaser.Pressure);
        var validator = new VCRCMitsubishiZubadanValidator();
        do
        {
            Point4 = Refrigerant.DewPointAt(IntermediatePressure);
            Point7 = Point6.IsenthalpicExpansionTo(RecuperatorHighPressure);
            Point8 = Refrigerant.BubblePointAt(RecuperatorHighPressure);
            Point9 = Point8.IsenthalpicExpansionTo(IntermediatePressure);
            Point11 = Point8.CoolingTo(Point9.Temperature + Economizer.TemperatureDifference);
            Point12 = Point11.IsenthalpicExpansionTo(Evaporator.Pressure);
            try
            {
                NewtonRaphson.FindRootNearGuess(
                    ToSolve, Differentiate.FirstDerivativeFunc(ToSolve), 80, 1e-9, 100 - 1e-9, 1e-3);
            }
            catch (Exception)
            {
                throw new ArgumentException("Solution not found!");
            }

            if (!validator.Validate(this).IsValid)
                RecuperatorHighPressure = CalculateIntermediatePressure(
                    RecuperatorHighPressure, Condenser.Pressure);
        } while (!validator.Validate(this).IsValid);
    }
}