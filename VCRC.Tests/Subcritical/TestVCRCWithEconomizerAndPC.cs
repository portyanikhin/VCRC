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

public static class TestVCRCWithEconomizerAndPC
{
    private const double Tolerance = 1e-10;

    private static readonly Refrigerant Refrigerant = new(FluidsList.R32);

    private static readonly Evaporator Evaporator =
        new(Refrigerant.Name, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Compressor Compressor = new(80.Percent());

    private static readonly Condenser Condenser =
        new(Refrigerant.Name, 45.DegreesCelsius(), TemperatureDelta.FromKelvins(3));

    private static readonly Economizer Economizer =
        new(TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));

    private static readonly VCRCWithEconomizerAndPC Cycle =
        new(Evaporator, Compressor, Condenser, Economizer);

    private static readonly EntropyAnalysisResult AnalysisResult =
        Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());

    [Test]
    public static void TestWrongEconomizerSuperheat()
    {
        Action action = () =>
            _ = new VCRCWithEconomizerAndPC(
                Evaporator, Compressor, Condenser,
                new Economizer(Economizer.TemperatureDifference,
                    TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at economizer 'hot' side!*");
    }

    [Test]
    public static void TestWrongEconomizerTemperatureDifference()
    {
        Action action = () =>
            _ = new VCRCWithEconomizerAndPC(
                Evaporator, Compressor, Condenser,
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
        Cycle.Condenser.Should().Be(Condenser);
        Cycle.GasCooler.Should().BeNull();
        Cycle.Economizer.Should().Be(Economizer);
    }

    [Test]
    public static void TestIsTranscritical() =>
        Cycle.IsTranscritical.Should().BeFalse();

    [Test]
    public static void TestPoint1()
    {
        Cycle.Point1.Should().Be(Evaporator.Outlet);
        Cycle.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void TestPoint2s()
    {
        Cycle.Point2s.Should().Be(
            Cycle.Point1.IsentropicCompressionTo(Condenser.Pressure));
        Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public static void TestPoint2()
    {
        Cycle.Point2.Should().Be(
            Cycle.Point1.CompressionTo(Condenser.Pressure,
                Compressor.Efficiency));
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public static void TestPoint3()
    {
        Cycle.Point3.Should().Be(Refrigerant.Superheated(
            Cycle.IntermediatePressure, Economizer.Superheat));
        Cycle.Point3.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void TestPoint4s()
    {
        Cycle.Point4s.Should().Be(
            Cycle.Point3.IsentropicCompressionTo(Condenser.Pressure));
        Cycle.Point4s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint4()
    {
        Cycle.Point4.Should().Be(
            Cycle.Point3.CompressionTo(Condenser.Pressure,
                Compressor.Efficiency));
        Cycle.Point4.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint5()
    {
        Cycle.Point5.Should().Be(
            Refrigerant.Mixing(Cycle.EvaporatorSpecificMassFlow, Cycle.Point2,
                Cycle.HeatReleaserSpecificMassFlow - Cycle.EvaporatorSpecificMassFlow, Cycle.Point4));
        Cycle.Point5.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public static void TestPoint6()
    {
        Cycle.Point6.Should().Be(Condenser.Outlet);
        Cycle.Point6.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public static void TestPoint7()
    {
        Cycle.Point7.Should().Be(
            Cycle.Point6.IsenthalpicExpansionTo(Cycle.IntermediatePressure));
        Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint8()
    {
        Cycle.Point8.Should().Be(
            Cycle.Point6.CoolingTo(
                Cycle.Point7.Temperature + Economizer.TemperatureDifference));
        Cycle.Point8.Phase.Should().Be(Phases.Liquid);
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
             (1 + (Cycle.Point6.Enthalpy - Cycle.Point8.Enthalpy) /
                 (Cycle.Point3.Enthalpy - Cycle.Point7.Enthalpy))).Percent,
            Tolerance);
        Cycle.IntermediateSpecificMassFlow.Should().Be(
            Cycle.HeatReleaserSpecificMassFlow - Cycle.EvaporatorSpecificMassFlow);
    }

    [Test]
    public static void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy +
            (Cycle.HeatReleaserSpecificMassFlow - Cycle.EvaporatorSpecificMassFlow).DecimalFractions *
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
            (Cycle.Point5.Enthalpy - Cycle.Point6.Enthalpy));

    [Test]
    public static void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            4.571161584395723, Tolerance);
    }

    [Test]
    public static void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            5.571161584395725, Tolerance);
    }

    [Test]
    public static void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(26.69062233718953, Tolerance);

    [Test]
    public static void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(26.672018619966924, Tolerance);

    [Test]
    public static void TestCompressorEnergyLossRatio() =>
        AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Tolerance);

    [Test]
    public static void TestCondenserEnergyLossRatio() =>
        AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(22.02518203021821, Tolerance);

    [Test]
    public static void TestGasCoolerEnergyLossRatio() =>
        AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(7.009215475275353, Tolerance);

    [Test]
    public static void TestEjectorEnergyLossRatio() =>
        AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(22.340527798473286, Tolerance);

    [Test]
    public static void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestEconomizerEnergyLossRatio() =>
        AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(1.7894781048364086, Tolerance);

    [Test]
    public static void TestMixingEnergyLossRatio() =>
        AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(0.16357797122982776, Tolerance);

    [Test]
    public static void TestAnalysisRelativeError()
    {
        AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.06974994089376046, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}