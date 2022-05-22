﻿using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Subcritical;

public class TestSimpleVCRC
{
    private const double Tolerance = 1e-10;
    private SimpleVCRC Cycle { get; set; } = null!;
    private EntropyAnalysisResult AnalysisResult { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        var evaporator = new Evaporator(FluidsList.R32, 5.DegreesCelsius(),
            TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(FluidsList.R32, 45.DegreesCelsius(),
            TemperatureDelta.FromKelvins(3));
        Cycle = new SimpleVCRC(evaporator, compressor, condenser);
        AnalysisResult = Cycle.EntropyAnalysis(
            18.DegreesCelsius(), 35.DegreesCelsius());
    }

    [TestCase(5, 4)]
    [TestCase(41, 40)]
    public void TestWrongTemperatures(double evaporatingTemperature, double condensingTemperature)
    {
        Action action = () =>
            _ = new SimpleVCRC(
                new Evaporator(Cycle.Evaporator.RefrigerantName,
                    evaporatingTemperature.DegreesCelsius(), Cycle.Evaporator.Superheat),
                Cycle.Compressor,
                new Condenser(Cycle.Condenser!.RefrigerantName,
                    condensingTemperature.DegreesCelsius(), Cycle.Condenser!.Subcooling));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Condensing temperature should be greater than evaporating temperature!*");
    }

    [Test]
    public void TestWrongRefrigerantNames()
    {
        Action action = () =>
            _ = new SimpleVCRC(
                new Evaporator(FluidsList.R22,
                    Cycle.Evaporator.Temperature, Cycle.Evaporator.Superheat),
                Cycle.Compressor, Cycle.Condenser!);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Only one refrigerant should be selected!*");
    }

    [TestCase(18, 18, "Indoor and outdoor temperatures should not be equal!")]
    [TestCase(13, 35, "Wrong temperature difference in the evaporator! Increase 'cold' source temperature.")]
    [TestCase(18, 42,
        "Wrong temperature difference in the condenser or gas cooler! Decrease 'hot' source temperature.")]
    public void TestEntropyAnalysisWrongTemperatures(double indoor, double outdoor, string message)
    {
        IEntropyAnalysable vcrc = Cycle;
        Action action = () =>
            vcrc.EntropyAnalysis(indoor.DegreesCelsius(), outdoor.DegreesCelsius());
        action.Should().Throw<ValidationException>().WithMessage($"*{message}*");
    }

    [Test]
    public void TestZeroSuperheat()
    {
        var vcrcWithZeroSuperheat =
            new SimpleVCRC(
                new Evaporator(Cycle.Evaporator.RefrigerantName,
                    Cycle.Evaporator.Temperature, TemperatureDelta.Zero),
                Cycle.Compressor, Cycle.Condenser!);
        vcrcWithZeroSuperheat.Point1.Pressure.Should().Be(
            vcrcWithZeroSuperheat.Evaporator.Pressure);
        vcrcWithZeroSuperheat.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestZeroSubcooling()
    {
        var vcrcWithZeroSubcooling =
            new SimpleVCRC(Cycle.Evaporator, Cycle.Compressor,
                new Condenser(Cycle.Condenser!.RefrigerantName,
                    Cycle.Condenser!.Temperature, TemperatureDelta.Zero));
        vcrcWithZeroSubcooling.Point3.Pressure.Should().Be(
            vcrcWithZeroSubcooling.Condenser!.Pressure);
        vcrcWithZeroSubcooling.Point3.Phase.Should().Be(Phases.TwoPhase);
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
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint2s()
    {
        Cycle.Point2s.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
        Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point2.Enthalpy.Should().Be(
            Cycle.Point1.Enthalpy + Cycle.SpecificWork);
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point3.Temperature.Should().Be(
            Cycle.Condenser!.Temperature - Cycle.Condenser!.Subcooling);
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
    public void TestHeatEmitter()
    {
        Cycle.Condenser.Should().NotBeNull();
        Cycle.GasCooler.Should().BeNull();
    }

    [Test]
    public void TestIsTranscritical() =>
        Cycle.IsTranscritical.Should().BeFalse();

    [Test]
    public void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy);

    [Test]
    public void TestSpecificWork() =>
        Cycle.SpecificWork.Should().Be(
            Cycle.IsentropicSpecificWork /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);

    [Test]
    public void TestSpecificCoolingCapacity() =>
        Cycle.SpecificCoolingCapacity.Should().Be(
            Cycle.Point1.Enthalpy - Cycle.Point4.Enthalpy);

    [Test]
    public void TestSpecificHeatingCapacity() =>
        Cycle.SpecificHeatingCapacity.Should().Be(
            Cycle.Point2.Enthalpy - Cycle.Point3.Enthalpy);

    [Test]
    public void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            4.326011919496399, Tolerance);
    }

    [Test]
    public void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            5.326011919496398, Tolerance);
    }

    [Test]
    public void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(25.259214367658867, Tolerance);

    [Test]
    public void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(25.259214367658878, Tolerance);

    [Test]
    public void TestCompressorEnergyLossRatio() =>
        AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Tolerance);

    [Test]
    public void TestCondenserEnergyLossRatio() =>
        AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(21.46877321647669, Tolerance);

    [Test]
    public void TestGasCoolerEnergyLossRatio() =>
        AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(12.141693490520616, Tolerance);

    [Test]
    public void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(21.13031892534382, Tolerance);

    [Test]
    public void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestEconomizerEnergyLossRatio() =>
        AnalysisResult.EconomizerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestMixingEnergyLossRatio() =>
        AnalysisResult.MixingEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestAnalysisRelativeError()
    {
        AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(6.236322468275591e-14, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}