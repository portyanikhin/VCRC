using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Subcritical;

public class TestVCRCMitsubishiZubadan
{
    private const double Tolerance = 1e-10;
    private VCRCMitsubishiZubadan Cycle { get; set; } = null!;
    private EntropyAnalysisResult AnalysisResult { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        var evaporator = new Evaporator(FluidsList.R32, 5.DegreesCelsius(),
            TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(FluidsList.R32, 45.DegreesCelsius(),
            TemperatureDelta.FromKelvins(3));
        var economizer = new EconomizerTPI(TemperatureDelta.FromKelvins(5));
        Cycle = new VCRCMitsubishiZubadan(evaporator, compressor, condenser, economizer);
        AnalysisResult = Cycle.EntropyAnalysis(
            18.DegreesCelsius(), 35.DegreesCelsius());
    }

    [Test]
    public void TestSolutionNotFound()
    {
        Action action = () =>
            _ = new VCRCMitsubishiZubadan(
                Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser,
                new EconomizerTPI(TemperatureDelta.FromKelvins(49)));
        action.Should().Throw<ArgumentException>().WithMessage("Solution not found!");
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
        Cycle.Point3.Enthalpy.JoulesPerKilogram.Should().BeApproximately(
            (Cycle.Point2.Enthalpy + (Cycle.Point3s.Enthalpy - Cycle.Point2.Enthalpy) /
                Cycle.Compressor.IsentropicEfficiency.DecimalFractions).JoulesPerKilogram, Tolerance);
        Cycle.Point3.Phase.Should().Be(Phases.Gas);
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
    public void TestHeatEmitter()
    {
        Cycle.Condenser.Should().NotBeNull();
        Cycle.GasCooler.Should().BeNull();
    }

    [Test]
    public void TestIsTranscritical() =>
        Cycle.IsTranscritical.Should().BeFalse();

    [Test]
    public void TestRecuperator() =>
        Cycle.Recuperator.TemperatureDifference
            .Should().Be(Cycle.Point7.Temperature - Cycle.Point2.Temperature);

    [Test]
    public void TestSpecificMassFlows()
    {
        Cycle.FirstStageSpecificMassFlow.Should().Be(100.Percent());
        Cycle.SecondStageSpecificMassFlow.Percent.Should().BeApproximately(
            (Cycle.FirstStageSpecificMassFlow * (1 + (Cycle.Point3.Enthalpy - Cycle.Point4.Enthalpy) /
                (Cycle.Point4.Enthalpy - Cycle.Point10.Enthalpy))).Percent, 1e-3);
    }

    [Test]
    public void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.Point3s.Enthalpy - Cycle.Point2.Enthalpy +
            Cycle.SecondStageSpecificMassFlow.DecimalFractions *
            (Cycle.Point5s.Enthalpy - Cycle.Point4.Enthalpy));

    [Test]
    public void TestSpecificWork() =>
        Cycle.SpecificWork.Should().Be(
            Cycle.IsentropicSpecificWork /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);

    [Test]
    public void TestSpecificCoolingCapacity() =>
        Cycle.SpecificCoolingCapacity.Should().Be(
            Cycle.Point1.Enthalpy - Cycle.Point12.Enthalpy);

    [Test]
    public void TestSpecificHeatingCapacity() =>
        Cycle.SpecificHeatingCapacity.Should().Be(
            Cycle.SecondStageSpecificMassFlow.DecimalFractions *
            (Cycle.Point5.Enthalpy - Cycle.Point6.Enthalpy));

    [Test]
    public void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            4.380342435725647, Tolerance);
    }

    [Test]
    public void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            5.380333104312685, Tolerance);
    }

    [Test]
    public void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(25.57644561474704, Tolerance);

    [Test]
    public void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(25.27093000190046, Tolerance);

    [Test]
    public void TestCompressorEnergyLossRatio() =>
        AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Tolerance);

    [Test]
    public void TestCondenserEnergyLossRatio() =>
        AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(17.402320942172274, Tolerance);

    [Test]
    public void TestGasCoolerEnergyLossRatio() =>
        AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(7.442291905240232, Tolerance);

    [Test]
    public void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(21.16669887701517, Tolerance);

    [Test]
    public void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should().BeApproximately(1.3431046427670745, Tolerance);

    [Test]
    public void TestEconomizerEnergyLossRatio() =>
        AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(0.44158251658286674, Tolerance);

    [Test]
    public void TestMixingEnergyLossRatio() =>
        AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(6.9330711143219235, Tolerance);

    [Test]
    public void TestAnalysisRelativeError()
    {
        AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(1.208960702370679, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}