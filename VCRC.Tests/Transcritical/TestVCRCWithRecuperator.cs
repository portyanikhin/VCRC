using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Transcritical;

public class TestVCRCWithRecuperator
{
    private const double Tolerance = 1e-10;
    private VCRCWithRecuperator Cycle { get; set; } = null!;
    private EntropyAnalysisResult AnalysisResult { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        var evaporator = new Evaporator(FluidsList.R744, 5.DegreesCelsius(),
            TemperatureDelta.FromKelvins(8));
        var recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
        var compressor = new Compressor(80.Percent());
        var gasCooler = new GasCooler(FluidsList.R744, 40.DegreesCelsius());
        Cycle = new VCRCWithRecuperator(evaporator, recuperator, compressor, gasCooler);
        AnalysisResult = Cycle.EntropyAnalysis(
            18.DegreesCelsius(), 35.DegreesCelsius());
    }

    [Test]
    public void TestWrongTemperatureDifferenceAtRecuperatorHotSide()
    {
        Action action = () =>
            _ = new VCRCWithRecuperator(
                Cycle.Evaporator,
                new Recuperator(TemperatureDelta.FromKelvins(49)),
                Cycle.Compressor, Cycle.GasCooler!);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Too high temperature difference at recuperator 'hot' side!*");
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
            Cycle.Point4.Temperature - Cycle.Recuperator.TemperatureDifference);
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint3s()
    {
        Cycle.Point3s.Pressure.Should().Be(Cycle.GasCooler!.Pressure);
        Cycle.Point3s.Entropy.Should().Be(Cycle.Point2.Entropy);
        Cycle.Point3s.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.GasCooler!.Pressure);
        Cycle.Point3.Enthalpy.Should().Be(
            Cycle.Point2.Enthalpy + Cycle.SpecificWork);
        Cycle.Point3.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.GasCooler!.Pressure);
        Cycle.Point4.Temperature.Should().Be(Cycle.GasCooler!.Temperature);
        Cycle.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.GasCooler!.Pressure);
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
    public void TestHeatEmitter()
    {
        Cycle.Condenser.Should().BeNull();
        Cycle.GasCooler.Should().NotBeNull();
    }

    [Test]
    public void TestIsTranscritical() =>
        Cycle.IsTranscritical.Should().BeTrue();

    [Test]
    public void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.Point3s.Enthalpy - Cycle.Point2.Enthalpy);

    [Test]
    public void TestSpecificWork() =>
        Cycle.SpecificWork.Should().Be(
            Cycle.IsentropicSpecificWork /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);

    [Test]
    public void TestSpecificCoolingCapacity() =>
        Cycle.SpecificCoolingCapacity.Should().Be(
            Cycle.Point1.Enthalpy - Cycle.Point6.Enthalpy);

    [Test]
    public void TestSpecificHeatingCapacity() =>
        Cycle.SpecificHeatingCapacity.Should().Be(
            Cycle.Point3.Enthalpy - Cycle.Point4.Enthalpy);

    [Test]
    public void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            2.711892365925208, Tolerance);
    }

    [Test]
    public void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            3.7118923659252077, Tolerance);
    }

    [Test]
    public void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(15.834508061387101, Tolerance);

    [Test]
    public void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(15.83450806138709, Tolerance);

    [Test]
    public void TestCompressorEnergyLossRatio() =>
        AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Tolerance);

    [Test]
    public void TestCondenserEnergyLossRatio() =>
        AnalysisResult.CondenserEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestGasCoolerEnergyLossRatio() =>
        AnalysisResult.GasCoolerEnergyLossRatio.Percent
            .Should().BeApproximately(29.670513651007557, Tolerance);

    [Test]
    public void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(19.10155564959979, Tolerance);

    [Test]
    public void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(13.037369549556171, Tolerance);

    [Test]
    public void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should().BeApproximately(2.356053088449407, Tolerance);

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
            .Should().BeApproximately(3.7834531981960063e-14, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}