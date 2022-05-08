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

namespace VCRC.Tests;

public class TestSimpleVCRC
{
    private SimpleVCRC Cycle { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        const FluidsList refrigerantName = FluidsList.R32;
        var evaporator = new Evaporator(refrigerantName, 5.DegreesCelsius(),
            TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(refrigerantName, 50.DegreesCelsius(),
            TemperatureDelta.FromKelvins(3));
        Cycle = new SimpleVCRC(evaporator, compressor, condenser);
    }

    [TestCase(5, 4)]
    [TestCase(51, 50)]
    public void TestWrongTemperatures(double evaporatingTemperature, double condensingTemperature)
    {
        Action action = () =>
            _ = new SimpleVCRC(
                new Evaporator(Cycle.RefrigerantName,
                    evaporatingTemperature.DegreesCelsius(), Cycle.Evaporator.Superheat),
                Cycle.Compressor,
                new Condenser(Cycle.RefrigerantName,
                    condensingTemperature.DegreesCelsius(), Cycle.Condenser.Subcooling));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Condensing temperature should be greater than evaporating temperature!*");
    }

    [Test]
    public void TestWrongRefrigerantNames()
    {
        Action action = () =>
            _ = new SimpleVCRC(
                new Evaporator(FluidsList.R32,
                    Cycle.Evaporator.Temperature, Cycle.Evaporator.Superheat),
                Cycle.Compressor,
                new Condenser(FluidsList.R410A,
                    Cycle.Condenser.Temperature, Cycle.Condenser.Subcooling));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Only one refrigerant should be selected!*");
    }

    [Test]
    public void TestZeroSuperheat()
    {
        var cycleWithZeroSuperheat =
            new SimpleVCRC(
                new Evaporator(Cycle.RefrigerantName,
                    Cycle.Evaporator.Temperature, TemperatureDelta.Zero),
                Cycle.Compressor, Cycle.Condenser);
        cycleWithZeroSuperheat.Point1
            .Should().Be(cycleWithZeroSuperheat.Evaporator.DewPoint);
    }

    [Test]
    public void TestZeroSubcooling()
    {
        var cycleWithZeroSubcooling =
            new SimpleVCRC(Cycle.Evaporator, Cycle.Compressor,
                new Condenser(Cycle.RefrigerantName,
                    Cycle.Condenser.Temperature, TemperatureDelta.Zero));
        cycleWithZeroSubcooling.Point3
            .Should().Be(cycleWithZeroSubcooling.Condenser.BubblePoint);
    }

    [Test]
    public void TestPoint1()
    {
        Cycle.Point1.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point1.Temperature.Should().Be(
            Cycle.Evaporator.DewPoint.Temperature + Cycle.Evaporator.Superheat);
        Cycle.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint2s()
    {
        Cycle.Point2s.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
        Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point2.Enthalpy.Should().Be(Cycle.Point1.Enthalpy + Cycle.SpecificWork);
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point3.Temperature.Should().Be(
            Cycle.Condenser.BubblePoint.Temperature - Cycle.Condenser.Subcooling);
        Cycle.Point3.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point4.Enthalpy.Should().Be(Cycle.Point3.Enthalpy);
        Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions * Cycle.SpecificWork);

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestEER() =>
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestCOP() =>
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);

    [TestCase(20, 20, "Indoor and outdoor temperatures should not be equal!")]
    [TestCase(5, 35, "Wrong temperature difference in the evaporator! Increase 'cold' source temperature.")]
    [TestCase(20, 50, "Wrong temperature difference in the condenser! Decrease 'hot' source temperature.")]
    public void TestEntropyAnalysisWrongTemperatures(double indoor, double outdoor, string message)
    {
        IEntropyAnalysable vcrc = Cycle;
        Action action = () =>
            vcrc.EntropyAnalysis(indoor.DegreesCelsius(), outdoor.DegreesCelsius());
        action.Should().Throw<ArgumentException>().WithMessage(message);
    }

    [Test]
    public void TestEntropyAnalysis()
    {
        var result =
            Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
        const double tolerance = 1e-10;
        result.ThermodynamicPerfection.Percent
            .Should().BeApproximately(21.600152157280235, tolerance);
        result.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(21.600152157280235, tolerance);
        result.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, tolerance);
        result.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(26.551093605669124, tolerance);
        result.GasCoolerEnergyLossRatio.Percent
            .Should().Be(0);
        result.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(13.789884357670854, tolerance);
        result.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(18.058869879379788, tolerance);
        result.RecuperatorEnergyLossRatio.Percent
            .Should().Be(0);
        result.EconomizerEnergyLossRatio.Percent
            .Should().Be(0);
        result.MixingEnergyLossRatio.Percent
            .Should().Be(0);
        result.AnalysisRelativeError.Percent
            .Should().BeApproximately(5.576481066223869e-14, tolerance);
        result.Sum().Percent
            .Should().BeApproximately(100, tolerance);
    }
}