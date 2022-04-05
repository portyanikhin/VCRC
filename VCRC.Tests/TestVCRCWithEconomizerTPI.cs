using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Tests;

public class TestVCRCWithEconomizerTPI
{
    private VCRCWithEconomizerTPI Cycle { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        const FluidsList refrigerantName = FluidsList.R32;
        var evaporator = new Evaporator(refrigerantName, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(refrigerantName, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));
        var economizer = new EconomizerTPI(evaporator, condenser, TemperatureDelta.FromKelvins(5));
        Cycle = new VCRCWithEconomizerTPI(evaporator, compressor, condenser, economizer);
    }

    [TestCase(Bound.Lower, "Intermediate pressure should be greater than evaporating pressure!")]
    [TestCase(Bound.Higher, "Intermediate pressure should be less than condensing pressure!")]
    public void TestWrongIntermediatePressure(Bound bound, string message)
    {
        Action action = () =>
            _ = new VCRCWithEconomizerTPI(Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser,
                new EconomizerTPI(bound is Bound.Lower
                        ? Cycle.Evaporator.Pressure
                        : Cycle.Condenser.Pressure,
                    Cycle.Economizer.TemperatureDifference));
        action.Should().Throw<ValidationException>().WithMessage($"*{message}*");
    }

    [Test]
    public void TestWrongEconomizerTemperatureDifference()
    {
        Action action = () =>
            _ = new VCRCWithEconomizerTPI(Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser,
                new EconomizerTPI(Cycle.Economizer.Pressure, TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Too high temperature difference at economizer 'cold' side!*");
    }

    [Test]
    public void TestSpecificMassFlows()
    {
        Cycle.FirstStageSpecificMassFlow.Should().Be(100.Percent());
        Cycle.SecondStageSpecificMassFlow.Should()
            .Be(Cycle.FirstStageSpecificMassFlow * (1 + (Cycle.Point2.Enthalpy - Cycle.Point3.Enthalpy) /
                (Cycle.Point3.Enthalpy - Cycle.Point9.Enthalpy)));
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint2s()
    {
        Cycle.Point2s.Pressure.Should().Be(Cycle.Economizer.Pressure);
        Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
        Cycle.Point2s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.Economizer.Pressure);
        Cycle.Point2.Enthalpy.Should()
            .Be(Cycle.Point1.Enthalpy + (Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy) /
                Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.Economizer.Pressure);
        Cycle.Point3.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
        Cycle.Point3.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint4s()
    {
        Cycle.Point4s.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point4s.Entropy.Should().Be(Cycle.Point3.Entropy);
        Cycle.Point4s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point4.Enthalpy.Should()
            .Be(Cycle.Point3.Enthalpy + (Cycle.Point4s.Enthalpy - Cycle.Point3.Enthalpy) /
                Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point4.Enthalpy.Should().BeGreaterThan(Cycle.Point4s.Enthalpy);
        Cycle.Point4.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point5.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
        Cycle.Point5.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint6()
    {
        Cycle.Point6.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point6.Quality.Should().Be(TwoPhase.Bubble.VaporQuality());
        Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint7()
    {
        Cycle.Point7.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point7.Temperature.Should().Be(Cycle.Point6.Temperature - Cycle.Condenser.Subcooling);
        Cycle.Point7.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public void TestPoint8()
    {
        Cycle.Point8.Pressure.Should().Be(Cycle.Economizer.Pressure);
        Cycle.Point8.Enthalpy.Should().Be(Cycle.Point7.Enthalpy);
        Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint9()
    {
        Cycle.Point9.Pressure.Should().Be(Cycle.Economizer.Pressure);
        Cycle.Point9.Enthalpy.Should()
            .Be(
                ((Cycle.Point8.Enthalpy.JoulesPerKilogram *
                  (Cycle.Point2.Enthalpy.JoulesPerKilogram - Cycle.Point3.Enthalpy.JoulesPerKilogram) +
                  Cycle.Point3.Enthalpy.JoulesPerKilogram *
                  (Cycle.Point7.Enthalpy.JoulesPerKilogram - Cycle.Point10.Enthalpy.JoulesPerKilogram)) /
                 (Cycle.Point2.Enthalpy.JoulesPerKilogram - Cycle.Point3.Enthalpy.JoulesPerKilogram +
                     Cycle.Point7.Enthalpy.JoulesPerKilogram - Cycle.Point10.Enthalpy.JoulesPerKilogram))
                .JoulesPerKilogram());
        Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint10()
    {
        Cycle.Point10.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point10.Temperature.Should()
            .Be(Cycle.Point8.Temperature + Cycle.Economizer.TemperatureDifference);
        Cycle.Point10.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public void TestPoint11()
    {
        Cycle.Point11.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point11.Enthalpy.Should().Be(Cycle.Point10.Enthalpy);
        Cycle.Point11.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestEntropyAnalysis()
    {
        var result = Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
        const double tolerance = 1e-10;
        result.ThermodynamicPerfection.Percent.Should().BeApproximately(23.441185685821615, tolerance);
        result.MinSpecificWorkRatio.Percent.Should().BeApproximately(23.296346713235014, tolerance);
        result.CompressorEnergyLossRatio.Percent.Should().BeApproximately(20, tolerance);
        result.CondenserEnergyLossRatio.Percent.Should().BeApproximately(23.88799133086776, tolerance);
        result.ExpansionValvesEnergyLossRatio.Percent.Should().BeApproximately(8.407988297423403, tolerance);
        result.EvaporatorEnergyLossRatio.Percent.Should().BeApproximately(19.50967120382623, tolerance);
        result.RecuperatorEnergyLossRatio.Percent.Should().Be(0);
        result.EconomizerEnergyLossRatio.Percent.Should().BeApproximately(2.1947355143709237, tolerance);
        result.MixingEnergyLossRatio.Percent.Should().BeApproximately(2.7032669402766745, tolerance);
        result.AnalysisRelativeError.Percent.Should().BeApproximately(0.6217239740182666, tolerance);
        result.Sum().Percent.Should().BeApproximately(100, tolerance);
    }
}