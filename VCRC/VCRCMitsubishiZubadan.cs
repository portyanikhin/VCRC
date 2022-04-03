using System;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.Units;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Validators;

namespace VCRC;

/// <summary>
///     Mitsubishi Zubadan VCRC.
/// </summary>
/// <remarks>
///     Two-stage VCRC with economizer, recuperator and two-phase injection to the compressor.
/// </remarks>
public class VCRCMitsubishiZubadan : TwoStageSubcriticalVCRC
{
    /// <summary>
    ///     Mitsubishi Zubadan VCRC.
    /// </summary>
    /// <remarks>
    ///     Two-stage VCRC with economizer, recuperator and two-phase injection to the compressor.
    /// </remarks>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="recuperator">Recuperator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="condenser">Condenser.</param>
    /// <param name="economizer">Economizer.</param>
    /// <exception cref="ArgumentException">Solution not found!</exception>
    /// <exception cref="ValidationException">Only one refrigerant should be selected!</exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Superheat in the recuperator should be greater than zero!
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
    ///     Intermediate pressure should be greater than evaporating pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Intermediate pressure should be less than condensing pressure!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     There should be a two-phase refrigerant at the compressor injection circuit!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at economizer 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Too high temperature difference at economizer 'cold' side!
    /// </exception>
    public VCRCMitsubishiZubadan(Evaporator evaporator, Recuperator recuperator, Compressor compressor,
        Condenser condenser, EconomizerTPI economizer) : base(evaporator, compressor, condenser)
    {
        Recuperator = recuperator;
        Economizer = economizer;
        Point2 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Input.Temperature(Point1.Temperature + Recuperator.Superheat));
        Point3s = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
            Input.Entropy(Point2.Entropy));
        var isentropicSpecificWork1 = Point3s.Enthalpy - Point2.Enthalpy;
        var specificWork1 = isentropicSpecificWork1 / Compressor.IsentropicEfficiency.DecimalFractions;
        Point3 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
            Input.Enthalpy(Point2.Enthalpy + specificWork1));
        Point4 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
            Input.Quality(TwoPhase.Dew.VaporQuality()));
        Point5s = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Entropy(Point4.Entropy));
        Point6 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Quality(TwoPhase.Dew.VaporQuality()));
        Point7 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Quality(TwoPhase.Bubble.VaporQuality()));
        Point8 = Condenser.Subcooling == TemperatureDelta.Zero
            ? Point7.Clone()
            : Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
                Input.Temperature(Point7.Temperature - Condenser.Subcooling));
        CalculateInjectionQuality(); // Also calculates Point9, Point10, Point11, Point12, Point13
        new VCRCMitsubishiZubadanValidator().ValidateAndThrow(this);
        Point14 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Input.Enthalpy(Point13!.Enthalpy));
        var isentropicSpecificWork2 =
            SecondStageSpecificMassFlow.DecimalFractions * (Point5s.Enthalpy - Point4.Enthalpy);
        var specificWork2 = isentropicSpecificWork2 / Compressor.IsentropicEfficiency.DecimalFractions;
        Point5 = Refrigerant.WithState(Input.Pressure(Condenser.Pressure),
            Input.Enthalpy(Point4.Enthalpy + specificWork2 / SecondStageSpecificMassFlow.DecimalFractions));
        IsentropicSpecificWork = isentropicSpecificWork1 + isentropicSpecificWork2;
        SpecificWork = specificWork1 + specificWork2;
        SpecificCoolingCapacity = Point1.Enthalpy - Point14.Enthalpy;
        SpecificHeatingCapacity =
            SecondStageSpecificMassFlow.DecimalFractions * (Point5.Enthalpy - Point8.Enthalpy);
    }

    /// <summary>
    ///     Recuperator as a VCRC component.
    /// </summary>
    public Recuperator Recuperator { get; }

    /// <summary>
    ///     Economizer as a VCRC component.
    /// </summary>
    public EconomizerTPI Economizer { get; }

    /// <summary>
    ///     Point 2 – recuperator "cold" outlet / first compression stage suction.
    /// </summary>
    public Refrigerant Point2 { get; }

    /// <summary>
    ///     Point 3s – first isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point3s { get; }

    /// <summary>
    ///     Point 3 – first compression stage discharge.
    /// </summary>
    public Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4 – second compression stage suction.
    /// </summary>
    public Refrigerant Point4 { get; }

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
    ///     Point 6 – dew-point on the condensing isobar.
    /// </summary>
    public Refrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – bubble-point on the condensing isobar.
    /// </summary>
    public Refrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – condenser outlet / first EV inlet.
    /// </summary>
    public Refrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – first EV outlet / recuperator "hot" inlet.
    /// </summary>
    public Refrigerant Point9 { get; private set; } = null!;

    /// <summary>
    ///     Point 10 – recuperator "hot" outlet / second EV inlet / economizer "hot" inlet.
    /// </summary>
    public Refrigerant Point10 { get; private set; } = null!;

    /// <summary>
    ///     Point 11 – second EV outlet / economizer "cold" inlet.
    /// </summary>
    public Refrigerant Point11 { get; private set; } = null!;

    /// <summary>
    ///     Point 12 – economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
    /// </summary>
    public Refrigerant Point12 { get; private set; } = null!;

    /// <summary>
    ///     Point 13 – economizer "hot" outlet / third EV inlet.
    /// </summary>
    public Refrigerant Point13 { get; private set; }

    /// <summary>
    ///     Point 14 – third EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point14 { get; }

    private Pressure RecuperatorHighPressure
    {
        get
        {
            double ToSolve(double pressure) =>
                (Refrigerant.WithState(Input.Pressure(pressure.Pascals()),
                     Input.Quality(TwoPhase.Bubble.VaporQuality())).Enthalpy -
                 (Point8.Enthalpy - FirstStageSpecificMassFlow / SecondStageSpecificMassFlow *
                     (Point2.Enthalpy - Point1.Enthalpy)))
                .JoulesPerKilogram;

            return Bisection.FindRoot(
                    ToSolve, Economizer.Pressure.Pascals, Condenser.Pressure.Pascals, 1e-2)
                .Pascals()
                .ToUnit(PressureUnit.Kilopascal);
        }
    }

    private void CalculateInjectionQuality()
    {
        double ToSolve(double injectionQuality) =>
            (CalculateEconomizerTemperatureDifference(injectionQuality.Percent()) -
             Economizer.TemperatureDifference).Kelvins;

        try
        {
            NewtonRaphson.FindRootNearGuess(
                ToSolve, Differentiate.FirstDerivativeFunc(ToSolve), 80, 1e-9, 100 - 1e-9, 1e-2);
        }
        catch (Exception)
        {
            throw new ArgumentException("Solution not found!");
        }
    }

    private TemperatureDelta CalculateEconomizerTemperatureDifference(Ratio injectionQuality)
    {
        Point12 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
            Input.Quality(injectionQuality));
        SecondStageSpecificMassFlow =
            FirstStageSpecificMassFlow *
            (1 + (Point3.Enthalpy - Point4.Enthalpy) / (Point4.Enthalpy - Point12.Enthalpy));
        Point9 = Refrigerant.WithState(Input.Pressure(RecuperatorHighPressure),
            Input.Enthalpy(Point8.Enthalpy));
        Point10 = Refrigerant.WithState(Input.Pressure(RecuperatorHighPressure),
            Input.Quality(TwoPhase.Bubble.VaporQuality()));
        Point11 = Refrigerant.WithState(Input.Pressure(Economizer.Pressure),
            Input.Enthalpy(Point10.Enthalpy));
        Point13 = Refrigerant.WithState(Input.Pressure(RecuperatorHighPressure),
            Input.Enthalpy(Point10.Enthalpy - (SecondStageSpecificMassFlow - FirstStageSpecificMassFlow) /
                FirstStageSpecificMassFlow * (Point12.Enthalpy - Point11.Enthalpy)));
        return Point13.Temperature - Point11.Temperature;
    }
}