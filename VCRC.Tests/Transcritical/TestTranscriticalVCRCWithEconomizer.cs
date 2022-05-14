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
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Transcritical;

namespace VCRC.Tests.Transcritical;

public class TestTranscriticalVCRCWithEconomizer
{
    private TranscriticalVCRCWithEconomizer Cycle { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        const FluidsList refrigerantName = FluidsList.R744;
        var evaporator = new Evaporator(refrigerantName,
            5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var gasCooler = new GasCooler(refrigerantName, 40.DegreesCelsius());
        var economizer = new Economizer(evaporator, gasCooler,
            TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
        Cycle = new TranscriticalVCRCWithEconomizer(evaporator, compressor, gasCooler, economizer);
    }

    [Test]
    public void TestWrongEconomizerSuperheat()
    {
        Action action = () =>
            _ = new TranscriticalVCRCWithEconomizer(
                Cycle.Evaporator, Cycle.Compressor, Cycle.GasCooler,
                new Economizer(Cycle.Economizer.Pressure,
                    Cycle.Economizer.TemperatureDifference, TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at economizer 'hot' side!*");
    }

    [Test]
    public void TestWrongEconomizerTemperatureDifference()
    {
        Action action = () =>
            _ = new TranscriticalVCRCWithEconomizer(
                Cycle.Evaporator, Cycle.Compressor, Cycle.GasCooler,
                new Economizer(Cycle.Economizer.Pressure,
                    TemperatureDelta.FromKelvins(50), Cycle.Economizer.Superheat));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Too high temperature difference at economizer 'cold' side!*");
    }

    [Test]
    public void TestSpecificMassFlows()
    {
        Cycle.FirstStageSpecificMassFlow.Should().Be(100.Percent());
        Cycle.SecondStageSpecificMassFlow.Should().Be(
            Cycle.FirstStageSpecificMassFlow * (1 + (Cycle.Point5.Enthalpy - Cycle.Point8.Enthalpy) /
                (Cycle.Point7.Enthalpy - Cycle.Point6.Enthalpy)));
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint2s()
    {
        Cycle.Point2s.Pressure.Should().Be(Cycle.Economizer.Pressure);
        Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
        Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.Economizer.Pressure);
        Cycle.Point2.Enthalpy.Should().Be(
            Cycle.Point1.Enthalpy + (Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy) /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.Economizer.Pressure);
        Cycle.Point3.Enthalpy.Should().Be(
            (Cycle.FirstStageSpecificMassFlow.DecimalFractions * Cycle.Point2.Enthalpy +
             (Cycle.SecondStageSpecificMassFlow - Cycle.FirstStageSpecificMassFlow).DecimalFractions *
             Cycle.Point7.Enthalpy) / Cycle.SecondStageSpecificMassFlow.DecimalFractions);
        Cycle.Point3.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint4s()
    {
        Cycle.Point4s.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point4s.Entropy.Should().Be(Cycle.Point3.Entropy);
        Cycle.Point4s.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point4.Enthalpy.Should().Be(
            Cycle.Point3.Enthalpy + (Cycle.Point4s.Enthalpy - Cycle.Point3.Enthalpy) /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point4.Enthalpy.Should().BeGreaterThan(Cycle.Point4s.Enthalpy);
        Cycle.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point5.Temperature.Should().Be(Cycle.GasCooler.Temperature);
        Cycle.Point5.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint6()
    {
        Cycle.Point6.Pressure.Should().Be(Cycle.Economizer.Pressure);
        Cycle.Point6.Enthalpy.Should().Be(Cycle.Point5.Enthalpy);
        Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint7()
    {
        Cycle.Point7.Pressure.Should().Be(Cycle.Economizer.Pressure);
        Cycle.Point7.Temperature.Should().Be(
            Cycle.Point7.WithState(Input.Pressure(Cycle.Economizer.Pressure),
                Input.Quality(TwoPhase.Dew.VaporQuality())).Temperature + Cycle.Economizer.Superheat);
        Cycle.Point7.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint8()
    {
        Cycle.Point8.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point8.Temperature.Should().Be(
            Cycle.Point6.Temperature + Cycle.Economizer.TemperatureDifference);
        Cycle.Point8.Phase.Should().Be(Phases.SupercriticalLiquid);
    }

    [Test]
    public void TestPoint9()
    {
        Cycle.Point9.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point9.Enthalpy.Should().Be(Cycle.Point8.Enthalpy);
        Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestEntropyAnalysis()
    {
        var result =
            Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
        const double tolerance = 1e-10;
        result.ThermodynamicPerfection.Percent
            .Should().BeApproximately(17.373450402746318, tolerance);
        result.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(17.29488083008007, tolerance);
        result.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, tolerance);
        result.CondenserEnergyLossRatio.Percent
            .Should().Be(0);
        result.GasCoolerEnergyLossRatio.Percent
            .Should().BeApproximately(22.894427549434845, tolerance);
        result.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(22.122480835651007, tolerance);
        result.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(14.268016858166629, tolerance);
        result.RecuperatorEnergyLossRatio.Percent
            .Should().Be(0);
        result.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(2.6005044065843, tolerance);
        result.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(0.8196895200831331, tolerance);
        result.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.4542938077352593, tolerance);
        result.Sum().Percent
            .Should().BeApproximately(100, tolerance);
    }
}