using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Subcritical;

public class TestVCRCWithEconomizer
{
    private const double Tolerance = 1e-10;
    private VCRCWithEconomizer Cycle { get; set; } = null!;
    private EntropyAnalysisResult AnalysisResult { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        var evaporator = new Evaporator(FluidsList.R32, 5.DegreesCelsius(),
            TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(FluidsList.R32, 45.DegreesCelsius(),
            TemperatureDelta.FromKelvins(3));
        var economizer = new Economizer(TemperatureDelta.FromKelvins(5),
            TemperatureDelta.FromKelvins(5));
        Cycle = new VCRCWithEconomizer(evaporator, compressor, condenser, economizer);
        AnalysisResult = Cycle.EntropyAnalysis(
            18.DegreesCelsius(), 35.DegreesCelsius());
    }

    [Test]
    public void TestWrongEconomizerSuperheat()
    {
        Action action = () =>
            _ = new VCRCWithEconomizer(
                Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser!,
                new Economizer(Cycle.Economizer.TemperatureDifference,
                    TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at economizer 'hot' side!*");
    }

    [Test]
    public void TestWrongEconomizerTemperatureDifference()
    {
        Action action = () =>
            _ = new VCRCWithEconomizer(
                Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser!,
                new Economizer(TemperatureDelta.FromKelvins(49),
                    Cycle.Economizer.Superheat));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Too high temperature difference at economizer 'cold' side!*");
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
        Cycle.Point2s.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
        Cycle.Point2s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point2.Enthalpy.Should().Be(
            Cycle.Point1.Enthalpy + (Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy) /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point3.Enthalpy.Should().Be(
            (Cycle.FirstStageSpecificMassFlow.DecimalFractions * Cycle.Point2.Enthalpy +
             (Cycle.SecondStageSpecificMassFlow - Cycle.FirstStageSpecificMassFlow).DecimalFractions *
             Cycle.Point7.Enthalpy) / Cycle.SecondStageSpecificMassFlow.DecimalFractions);
        Cycle.Point3.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint4s()
    {
        Cycle.Point4s.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point4s.Entropy.Should().Be(Cycle.Point3.Entropy);
        Cycle.Point4s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point4.Enthalpy.Should().Be(
            Cycle.Point3.Enthalpy + (Cycle.Point4s.Enthalpy - Cycle.Point3.Enthalpy) /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point4.Enthalpy.Should().BeGreaterThan(Cycle.Point4s.Enthalpy);
        Cycle.Point4.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point5.Temperature.Should().Be(
            Cycle.Condenser!.Temperature - Cycle.Condenser!.Subcooling);
        Cycle.Point5.Phase.Should().Be(Phases.Liquid);
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
        Cycle.Point7.Temperature.Should().Be(
            Cycle.Point7.WithState(Input.Pressure(Cycle.IntermediatePressure),
                Input.Quality(TwoPhase.Dew.VaporQuality())).Temperature + Cycle.Economizer.Superheat);
        Cycle.Point7.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint8()
    {
        Cycle.Point8.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point8.Temperature.Should().Be(
            Cycle.Point6.Temperature + Cycle.Economizer.TemperatureDifference);
        Cycle.Point8.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public void TestPoint9()
    {
        Cycle.Point9.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point9.Enthalpy.Should().Be(Cycle.Point8.Enthalpy);
        Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
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
    public void TestSpecificMassFlows()
    {
        Cycle.FirstStageSpecificMassFlow.Should().Be(100.Percent());
        Cycle.SecondStageSpecificMassFlow.Should().Be(
            Cycle.FirstStageSpecificMassFlow * (1 + (Cycle.Point5.Enthalpy - Cycle.Point8.Enthalpy) /
                (Cycle.Point7.Enthalpy - Cycle.Point6.Enthalpy)));
    }

    [Test]
    public void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy +
            Cycle.SecondStageSpecificMassFlow.DecimalFractions *
            (Cycle.Point4s.Enthalpy - Cycle.Point3.Enthalpy));

    [Test]
    public void TestSpecificWork() =>
        Cycle.SpecificWork.Should().Be(
            Cycle.IsentropicSpecificWork /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);

    [Test]
    public void TestSpecificCoolingCapacity() =>
        Cycle.SpecificCoolingCapacity.Should().Be(
            Cycle.Point1.Enthalpy - Cycle.Point9.Enthalpy);

    [Test]
    public void TestSpecificHeatingCapacity() =>
        Cycle.SpecificHeatingCapacity.Should().Be(
            Cycle.SecondStageSpecificMassFlow.DecimalFractions *
            (Cycle.Point4.Enthalpy - Cycle.Point5.Enthalpy));

    [Test]
    public void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            4.511109316237719, Tolerance);
    }

    [Test]
    public void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            5.511109316237718, Tolerance);
    }

    [Test]
    public void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(26.339982268947697, Tolerance);

    [Test]
    public void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(26.213423177542623, Tolerance);

    [Test]
    public void TestCompressorEnergyLossRatio() =>
        AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Tolerance);

    [Test]
    public void TestCondenserEnergyLossRatio() =>
        AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(23.01216380485239, Tolerance);

    [Test]
    public void TestGasCoolerEnergyLossRatio() =>
        AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(6.888699877347386, Tolerance);

    [Test]
    public void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(21.956407482133088, Tolerance);

    [Test]
    public void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestEconomizerEnergyLossRatio() =>
        AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(1.7587100360640768, Tolerance);

    [Test]
    public void TestMixingEnergyLossRatio() =>
        AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(0.1705956220604388, Tolerance);

    [Test]
    public void TestAnalysisRelativeError()
    {
        AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.4828026105094935, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}