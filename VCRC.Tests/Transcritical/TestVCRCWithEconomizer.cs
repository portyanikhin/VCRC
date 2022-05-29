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

public static class TestVCRCWithEconomizer
{
    private const double Tolerance = 1e-10;

    private static readonly Refrigerant Refrigerant = new(FluidsList.R744);

    private static readonly Evaporator Evaporator =
        new(Refrigerant.Name, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Compressor Compressor = new(80.Percent());

    private static readonly GasCooler GasCooler = new(Refrigerant.Name, 40.DegreesCelsius());

    private static readonly Economizer Economizer =
        new(TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));

    private static readonly VCRCWithEconomizer Cycle =
        new(Evaporator, Compressor, GasCooler, Economizer);

    private static readonly EntropyAnalysisResult AnalysisResult =
        Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());

    [Test]
    public static void TestWrongEconomizerSuperheat()
    {
        Action action = () =>
            _ = new VCRCWithEconomizer(
                Evaporator, Compressor, GasCooler,
                new Economizer(Economizer.TemperatureDifference,
                    TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at economizer 'hot' side!*");
    }

    [Test]
    public static void TestWrongEconomizerTemperatureDifference()
    {
        Action action = () =>
            _ = new VCRCWithEconomizer(
                Evaporator, Compressor, GasCooler,
                new Economizer(TemperatureDelta.FromKelvins(49),
                    Economizer.Superheat));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Too high temperature difference at economizer 'cold' side!*");
    }

    [Test]
    public static void TestComponents()
    {
        Cycle.Evaporator.Should().Be(Evaporator);
        Cycle.Compressor.Should().Be(Compressor);
        Cycle.Condenser.Should().BeNull();
        Cycle.GasCooler.Should().Be(GasCooler);
        Cycle.Economizer.Should().Be(Economizer);
    }

    [Test]
    public static void TestIsTranscritical() =>
        Cycle.IsTranscritical.Should().BeTrue();

    [Test]
    public static void TestPoint1()
    {
        Cycle.Point1.Should().Be(
            Refrigerant.DewPointAt(Evaporator.Temperature)
                .HeatingTo(Evaporator.Temperature + Evaporator.Superheat));
        Cycle.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void TestPoint2s()
    {
        Cycle.Point2s.Should().Be(
            Cycle.Point1.IsentropicCompressionTo(Cycle.IntermediatePressure));
        Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public static void TestPoint2()
    {
        Cycle.Point2.Should().Be(
            Cycle.Point1.CompressionTo(Cycle.IntermediatePressure,
                Compressor.Efficiency));
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public static void TestPoint3()
    {
        Cycle.Point3.Should().Be(
            Refrigerant.Mixing(Cycle.EvaporatorSpecificMassFlow, Cycle.Point2,
                Cycle.HeatReleaserSpecificMassFlow - Cycle.EvaporatorSpecificMassFlow, Cycle.Point7));
        Cycle.Point3.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void TestPoint4s()
    {
        Cycle.Point4s.Should().Be(
            Cycle.Point3.IsentropicCompressionTo(GasCooler.Pressure));
        Cycle.Point4s.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public static void TestPoint4()
    {
        Cycle.Point4.Should().Be(
            Cycle.Point3.CompressionTo(GasCooler.Pressure,
                Compressor.Efficiency));
        Cycle.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public static void TestPoint5()
    {
        Cycle.Point5.Should().Be(
            Refrigerant.WithState(Input.Pressure(GasCooler.Pressure),
                Input.Temperature(GasCooler.Temperature)));
        Cycle.Point5.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public static void TestPoint6()
    {
        Cycle.Point6.Should().Be(
            Cycle.Point5.IsenthalpicExpansionTo(Cycle.IntermediatePressure));
        Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint7()
    {
        Cycle.Point7.Should().Be(
            Cycle.Point6.HeatingTo(
                Refrigerant.DewPointAt(Cycle.IntermediatePressure).Temperature +
                Economizer.Superheat));
        Cycle.Point7.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint8()
    {
        Cycle.Point8.Should().Be(
            Cycle.Point5.CoolingTo(
                Cycle.Point6.Temperature + Economizer.TemperatureDifference));
        Cycle.Point8.Phase.Should().Be(Phases.SupercriticalLiquid);
    }

    [Test]
    public static void TestPoint9()
    {
        Cycle.Point9.Should().Be(
            Cycle.Point8.IsenthalpicExpansionTo(Evaporator.Pressure));
        Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestSpecificMassFlows()
    {
        Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Cycle.HeatReleaserSpecificMassFlow.Percent.Should().BeApproximately(
            (Cycle.EvaporatorSpecificMassFlow *
             (1 + (Cycle.Point5.Enthalpy - Cycle.Point8.Enthalpy) /
                 (Cycle.Point7.Enthalpy - Cycle.Point6.Enthalpy))).Percent,
            Tolerance);
    }

    [Test]
    public static void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy +
            Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Cycle.Point4s.Enthalpy - Cycle.Point3.Enthalpy));

    [Test]
    public static void TestSpecificWork() =>
        Cycle.SpecificWork.Should().Be(
            Cycle.IsentropicSpecificWork /
            Compressor.Efficiency.DecimalFractions);

    [Test]
    public static void TestSpecificCoolingCapacity() =>
        Cycle.SpecificCoolingCapacity.Should().Be(
            Cycle.Point1.Enthalpy - Cycle.Point9.Enthalpy);

    [Test]
    public static void TestSpecificHeatingCapacity() =>
        Cycle.SpecificHeatingCapacity.Should().Be(
            Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Cycle.Point4.Enthalpy - Cycle.Point5.Enthalpy));

    [Test]
    public static void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            2.975458873387994, Tolerance);
    }

    [Test]
    public static void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            3.9754588733879945, Tolerance);
    }

    [Test]
    public static void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(17.373450402746318, Tolerance);

    [Test]
    public static void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(17.29488083008007, Tolerance);

    [Test]
    public static void TestCompressorEnergyLossRatio() =>
        AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Tolerance);

    [Test]
    public static void TestCondenserEnergyLossRatio() =>
        AnalysisResult.CondenserEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestGasCoolerEnergyLossRatio() =>
        AnalysisResult.GasCoolerEnergyLossRatio.Percent
            .Should().BeApproximately(22.894427549434845, Tolerance);

    [Test]
    public static void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(22.122480835651007, Tolerance);

    [Test]
    public static void TestEjectorEnergyLossRatio() =>
        AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(14.268016858166629, Tolerance);

    [Test]
    public static void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestEconomizerEnergyLossRatio() =>
        AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(2.6005044065843, Tolerance);

    [Test]
    public static void TestMixingEnergyLossRatio() =>
        AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(0.8196895200831331, Tolerance);

    [Test]
    public static void TestAnalysisRelativeError()
    {
        AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.4542938077352593, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}