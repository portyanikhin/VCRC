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
using VCRC.Transcritical;

namespace VCRC.Tests.Transcritical;

public class TestTranscriticalVCRCWithEconomizerTPI
{
    private TranscriticalVCRCWithEconomizerTPI Cycle { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        const FluidsList refrigerantName = FluidsList.R744;
        var evaporator = new Evaporator(refrigerantName,
            5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var gasCooler = new GasCooler(refrigerantName, 40.DegreesCelsius());
        var economizer = new EconomizerTPI(TemperatureDelta.FromKelvins(5));
        Cycle = new TranscriticalVCRCWithEconomizerTPI(evaporator, compressor, gasCooler, economizer);
    }

    [Test]
    public void TestWrongEconomizerTemperatureDifference()
    {
        Action action = () =>
            _ = new TranscriticalVCRCWithEconomizerTPI(
                Cycle.Evaporator, Cycle.Compressor, Cycle.GasCooler,
                new EconomizerTPI(TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Too high temperature difference at economizer 'cold' side!*");
    }

    [Test]
    public void TestSpecificMassFlows()
    {
        Cycle.FirstStageSpecificMassFlow.Should().Be(100.Percent());
        Cycle.SecondStageSpecificMassFlow.Should().Be(
            Cycle.FirstStageSpecificMassFlow * (1 + (Cycle.Point2.Enthalpy - Cycle.Point3.Enthalpy) /
                (Cycle.Point3.Enthalpy - Cycle.Point7.Enthalpy)));
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint2s()
    {
        Cycle.Point2s.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
        Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point2.Enthalpy.Should().Be(
            Cycle.Point1.Enthalpy + (Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy) /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point3.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
        Cycle.Point3.Phase.Should().Be(Phases.TwoPhase);
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
        Cycle.Point6.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point6.Enthalpy.Should().Be(Cycle.Point5.Enthalpy);
        Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint7()
    {
        Cycle.Point7.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point7.Enthalpy.Should().Be(
            ((Cycle.Point6.Enthalpy.JoulesPerKilogram *
              (Cycle.Point2.Enthalpy.JoulesPerKilogram - Cycle.Point3.Enthalpy.JoulesPerKilogram) +
              Cycle.Point3.Enthalpy.JoulesPerKilogram *
              (Cycle.Point5.Enthalpy.JoulesPerKilogram - Cycle.Point8.Enthalpy.JoulesPerKilogram)) /
             (Cycle.Point2.Enthalpy.JoulesPerKilogram - Cycle.Point3.Enthalpy.JoulesPerKilogram +
                 Cycle.Point5.Enthalpy.JoulesPerKilogram - Cycle.Point8.Enthalpy.JoulesPerKilogram))
            .JoulesPerKilogram());
        Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
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
            .Should().BeApproximately(16.193152260389702, tolerance);
        result.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(16.124874623066734, tolerance);
        result.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, tolerance);
        result.CondenserEnergyLossRatio.Percent
            .Should().Be(0);
        result.GasCoolerEnergyLossRatio.Percent
            .Should().BeApproximately(13.841278779116035, tolerance);
        result.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(30.34923839135392, tolerance);
        result.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(13.302779314766417, tolerance);
        result.RecuperatorEnergyLossRatio.Percent
            .Should().Be(0);
        result.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(2.533522866403796, tolerance);
        result.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(3.8483060252931014, tolerance);
        result.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.4234305005094076, tolerance);
        result.Sum().Percent
            .Should().BeApproximately(100, tolerance);
    }
}