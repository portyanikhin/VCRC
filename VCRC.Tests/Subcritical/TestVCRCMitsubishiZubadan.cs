using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Subcritical;

namespace VCRC.Tests.Subcritical;

public class TestVCRCMitsubishiZubadan
{
    private VCRCMitsubishiZubadan Cycle { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        const FluidsList refrigerantName = FluidsList.R32;
        var evaporator = new Evaporator(refrigerantName,
            5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(refrigerantName,
            50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));
        var economizer = new EconomizerTPI(TemperatureDelta.FromKelvins(5));
        Cycle = new VCRCMitsubishiZubadan(evaporator, compressor, condenser, economizer);
    }

    [Test]
    public void TestSolutionNotFound()
    {
        Action action = () =>
            _ = new VCRCMitsubishiZubadan(
                Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser,
                new EconomizerTPI(TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ArgumentException>().WithMessage("Solution not found!");
    }

    [Test]
    public void TestSpecificMassFlows()
    {
        Cycle.FirstStageSpecificMassFlow.Should().Be(100.Percent());
        Cycle.SecondStageSpecificMassFlow.Percent.Should().BeApproximately(
            (Cycle.FirstStageSpecificMassFlow * (1 + (Cycle.Point3.Enthalpy - Cycle.Point4.Enthalpy) /
                (Cycle.Point4.Enthalpy - Cycle.Point10.Enthalpy))).Percent, 1e-3);
    }

    [Test]
    public void TestRecuperator() =>
        Cycle.Recuperator.TemperatureDifference
            .Should().Be(Cycle.Point7.Temperature - Cycle.Point2.Temperature);

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point2.Enthalpy.Should().Be(
            Cycle.Point1.Enthalpy +
            Cycle.SecondStageSpecificMassFlow / Cycle.FirstStageSpecificMassFlow *
            (Cycle.Point7.Enthalpy - Cycle.Point8.Enthalpy));
        Cycle.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint3s()
    {
        Cycle.Point3s.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point3s.Entropy.Should().Be(Cycle.Point2.Entropy);
        Cycle.Point3s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point3.Enthalpy.Should().Be(
            Cycle.Point2.Enthalpy + (Cycle.Point3s.Enthalpy - Cycle.Point2.Enthalpy) /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point3.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point4.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
        Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint5s()
    {
        Cycle.Point5s.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point5s.Entropy.Should().Be(Cycle.Point4.Entropy);
        Cycle.Point5s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point5.Enthalpy.Should().Be(
            Cycle.Point4.Enthalpy + (Cycle.Point5s.Enthalpy - Cycle.Point4.Enthalpy) /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point5.Enthalpy.Should().BeGreaterThan(Cycle.Point5s.Enthalpy);
        Cycle.Point5.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint6()
    {
        Cycle.Point6.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point6.Temperature.Should().Be(
            Cycle.Condenser.Temperature - Cycle.Condenser.Subcooling);
        Cycle.Point6.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public void TestPoint7()
    {
        Cycle.Point7.Pressure.Should().Be(Cycle.RecuperatorHighPressure);
        Cycle.RecuperatorHighPressure.Should().BeGreaterThan(Cycle.IntermediatePressure);
        Cycle.RecuperatorHighPressure.Should().BeLessThan(Cycle.Condenser.Pressure);
        Cycle.Point7.Enthalpy.Should().Be(Cycle.Point6.Enthalpy);
        Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint8()
    {
        Cycle.Point8.Pressure.Should().Be(Cycle.RecuperatorHighPressure);
        Cycle.Point8.Quality.Should().Be(TwoPhase.Bubble.VaporQuality());
        Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint9()
    {
        Cycle.Point9.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point9.Enthalpy.Should().Be(Cycle.Point8.Enthalpy);
        Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint10()
    {
        Cycle.Point10.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point10.Enthalpy.JoulesPerKilogram.Should().BeApproximately(
            (Cycle.Point4.Enthalpy - Cycle.FirstStageSpecificMassFlow /
                (Cycle.SecondStageSpecificMassFlow - Cycle.FirstStageSpecificMassFlow) *
                (Cycle.Point3.Enthalpy - Cycle.Point4.Enthalpy)).JoulesPerKilogram, 10);
        Cycle.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint11()
    {
        Cycle.Point11.Pressure.Should().Be(Cycle.Point8.Pressure);
        Cycle.Point11.Enthalpy.Should().Be(
            Cycle.Point8.Enthalpy - (Cycle.SecondStageSpecificMassFlow - Cycle.FirstStageSpecificMassFlow) /
            Cycle.FirstStageSpecificMassFlow * (Cycle.Point10.Enthalpy - Cycle.Point9.Enthalpy));
        Cycle.Point11.Temperature.Kelvins.Should()
            .BeApproximately((Cycle.Point9.Temperature + Cycle.Economizer.TemperatureDifference).Kelvins, 1e-3);
        Cycle.Point11.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public void TestPoint12()
    {
        Cycle.Point12.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point12.Enthalpy.Should().Be(Cycle.Point11.Enthalpy);
        Cycle.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestEntropyAnalysis()
    {
        var result =
            Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
        const double tolerance = 1e-10;
        result.ThermodynamicPerfection.Percent
            .Should().BeApproximately(21.846548610045122, tolerance);
        result.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(21.520945255584795, tolerance);
        result.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, tolerance);
        result.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(22.388628935988034, tolerance);
        result.GasCoolerEnergyLossRatio.Percent
            .Should().Be(0);
        result.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(7.988361365628993, tolerance);
        result.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(18.02251639450806, tolerance);
        result.RecuperatorEnergyLossRatio.Percent
            .Should().BeApproximately(1.5664926521566622, tolerance);
        result.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(0.5205899175529854, tolerance);
        result.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(7.992465478580449, tolerance);
        result.AnalysisRelativeError.Percent
            .Should().BeApproximately(1.512960284009018, tolerance);
        result.Sum().Percent
            .Should().BeApproximately(100, tolerance);
    }
}