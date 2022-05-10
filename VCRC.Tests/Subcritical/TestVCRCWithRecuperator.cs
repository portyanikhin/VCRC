using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Subcritical;

namespace VCRC.Tests.Subcritical;

public class TestVCRCWithRecuperator
{
    private VCRCWithRecuperator Cycle { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        const FluidsList refrigerantName = FluidsList.R32;
        var evaporator = new Evaporator(refrigerantName,
            5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
        var recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(refrigerantName,
            50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));
        Cycle = new VCRCWithRecuperator(evaporator, recuperator, compressor, condenser);
    }

    [Test]
    public void TestWrongTemperatureDifferenceAtRecuperatorHotSide()
    {
        Action action = () =>
            _ = new VCRCWithRecuperator(
                new Evaporator(Cycle.Evaporator.RefrigerantName,
                    Cycle.Evaporator.Temperature, TemperatureDelta.FromKelvins(50)),
                Cycle.Recuperator, Cycle.Compressor, Cycle.Condenser);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at recuperator 'hot' side!*");
    }

    [Test]
    public void TestWrongTemperatureDifferenceAtRecuperatorColdSide()
    {
        Action action = () =>
            _ = new VCRCWithRecuperator(
                Cycle.Evaporator, Cycle.Recuperator, Cycle.Compressor,
                new Condenser(Cycle.Condenser.RefrigerantName,
                    Cycle.Condenser.Temperature, TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at recuperator 'cold' side!*");
    }

    [Test]
    public void TestZeroEvaporatorSuperheat()
    {
        var cycleZeroEvaporatorSuperheat =
            new VCRCWithRecuperator(
                new Evaporator(Cycle.Evaporator.RefrigerantName,
                    Cycle.Evaporator.Temperature, TemperatureDelta.Zero),
                Cycle.Recuperator, Cycle.Compressor, Cycle.Condenser);
        cycleZeroEvaporatorSuperheat.Point1.Pressure.Should().Be(
            cycleZeroEvaporatorSuperheat.Evaporator.Pressure);
        cycleZeroEvaporatorSuperheat.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestZeroRecuperatorSuperheat()
    {
        var cycleZeroRecuperatorSuperheat =
            new VCRCWithRecuperator(Cycle.Evaporator,
                new Recuperator(TemperatureDelta.Zero),
                Cycle.Compressor, Cycle.Condenser);
        cycleZeroRecuperatorSuperheat.Point2
            .Should().Be(cycleZeroRecuperatorSuperheat.Point1);
    }

    [Test]
    public void TestZeroSubcooling()
    {
        var cycleWithZeroSubcooling =
            new VCRCWithRecuperator(
                Cycle.Evaporator, Cycle.Recuperator, Cycle.Compressor,
                new Condenser(Cycle.Condenser.RefrigerantName,
                    Cycle.Condenser.Temperature, TemperatureDelta.Zero));
        cycleWithZeroSubcooling.Point4.Pressure.Should().Be(
            cycleWithZeroSubcooling.Condenser.Pressure);
        cycleWithZeroSubcooling.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint1()
    {
        Cycle.Point1.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point1.Temperature.Should().Be(
            Cycle.Evaporator.Temperature + Cycle.Evaporator.Superheat);
        Cycle.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point2.Temperature.Should().Be(
            Cycle.Point1.Temperature + Cycle.Recuperator.Superheat);
        Cycle.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint3s()
    {
        Cycle.Point3s.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point3s.Entropy.Should().Be(Cycle.Point2.Entropy);
        Cycle.Point3s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point3.Enthalpy.Should().Be(Cycle.Point2.Enthalpy + Cycle.SpecificWork);
        Cycle.Point3.Phase.Should().Be(Phases.SupercriticalGas);
        Cycle.Point3.Temperature.Should().BeGreaterThan(Cycle.Point3.CriticalTemperature);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point4.Temperature.Should().Be(
            Cycle.Condenser.Temperature - Cycle.Condenser.Subcooling);
        Cycle.Point4.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point5.Enthalpy.Should().Be(
            Cycle.Point4.Enthalpy - (Cycle.Point2.Enthalpy - Cycle.Point1.Enthalpy));
        Cycle.Point5.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public void TestPoint6()
    {
        Cycle.Point6.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point6.Enthalpy.Should().Be(Cycle.Point5.Enthalpy);
        Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestEntropyAnalysis()
    {
        var result =
            Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
        const double tolerance = 1e-10;
        result.ThermodynamicPerfection.Percent
            .Should().BeApproximately(21.45824801978035, tolerance);
        result.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(21.45824801978035, tolerance);
        result.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, tolerance);
        result.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(27.58536859968224, tolerance);
        result.GasCoolerEnergyLossRatio.Percent
            .Should().Be(0);
        result.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(12.110197168311025, tolerance);
        result.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(17.946008453548735, tolerance);
        result.RecuperatorEnergyLossRatio.Percent
            .Should().BeApproximately(0.9001777586776314, tolerance);
        result.EconomizerEnergyLossRatio.Percent
            .Should().Be(0);
        result.MixingEnergyLossRatio.Percent
            .Should().Be(0);
        result.AnalysisRelativeError.Percent
            .Should().BeApproximately(1.3514576635545213e-14, tolerance);
        result.Sum().Percent
            .Should().BeApproximately(100, tolerance);
    }
}