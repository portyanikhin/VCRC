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
using VCRC.Transcritical;

namespace VCRC.Tests.Transcritical;

public class TestTranscriticalVCRCWithRecuperator
{
    private TranscriticalVCRCWithRecuperator Cycle { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        const FluidsList refrigerantName = FluidsList.R744;
        var evaporator = new Evaporator(refrigerantName,
            5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
        var recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
        var compressor = new Compressor(80.Percent());
        var gasCooler = new GasCooler(refrigerantName, 40.DegreesCelsius());
        Cycle = new TranscriticalVCRCWithRecuperator(evaporator, recuperator, compressor, gasCooler);
    }

    [Test]
    public void TestWrongTemperatureDifferenceAtRecuperatorHotSide()
    {
        Action action = () =>
            _ = new TranscriticalVCRCWithRecuperator(
                new Evaporator(Cycle.Evaporator.RefrigerantName,
                    Cycle.Evaporator.Temperature, TemperatureDelta.FromKelvins(50)),
                Cycle.Recuperator, Cycle.Compressor, Cycle.GasCooler);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at recuperator 'hot' side!*");
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
        Cycle.Point3s.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point3s.Entropy.Should().Be(Cycle.Point2.Entropy);
        Cycle.Point3s.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point3.Enthalpy.Should().Be(Cycle.Point2.Enthalpy + Cycle.SpecificWork);
        Cycle.Point3.Phase.Should().Be(Phases.Supercritical);
        Cycle.Point3.Temperature.Should().BeGreaterThan(Cycle.Point3.CriticalTemperature);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point4.Temperature.Should().Be(Cycle.GasCooler.Temperature);
        Cycle.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point5.Enthalpy.Should().Be(
            Cycle.Point4.Enthalpy - (Cycle.Point2.Enthalpy - Cycle.Point1.Enthalpy));
        Cycle.Point5.Phase.Should().Be(Phases.Supercritical);
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
            .Should().BeApproximately(15.46558252987322, tolerance);
        result.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(15.46558252987322, tolerance);
        result.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, tolerance);
        result.CondenserEnergyLossRatio.Percent
            .Should().Be(0);
        result.GasCoolerEnergyLossRatio.Percent
            .Should().BeApproximately(23.765295685509226, tolerance);
        result.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(26.84936004509133, tolerance);
        result.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(12.67052010368821, tolerance);
        result.RecuperatorEnergyLossRatio.Percent
            .Should().BeApproximately(1.2492416358379914, tolerance);
        result.EconomizerEnergyLossRatio.Percent
            .Should().Be(0);
        result.MixingEnergyLossRatio.Percent
            .Should().Be(0);
        result.AnalysisRelativeError.Percent
            .Should().BeApproximately(3.406746531625435e-14, tolerance);
        result.Sum().Percent
            .Should().BeApproximately(100, tolerance);
    }
}