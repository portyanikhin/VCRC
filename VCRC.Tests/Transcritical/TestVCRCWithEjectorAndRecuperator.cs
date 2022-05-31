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

public static class TestVCRCWithEjectorAndRecuperator
{
    private const double Tolerance = 1e-6;

    private static readonly Refrigerant Refrigerant = new(FluidsList.R744);

    private static readonly Evaporator Evaporator =
        new(Refrigerant.Name, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Recuperator Recuperator = new(TemperatureDelta.FromKelvins(5));

    private static readonly Compressor Compressor = new(80.Percent());

    private static readonly GasCooler GasCooler =
        new(Refrigerant.Name, 40.DegreesCelsius());

    private static readonly Ejector Ejector =
        new(90.Percent(), 90.Percent(), 80.Percent());

    private static readonly VCRCWithEjectorAndRecuperator Cycle =
        new(Evaporator, Recuperator, Compressor, GasCooler, Ejector);

    private static readonly EntropyAnalysisResult AnalysisResult =
        Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());

    private static readonly EjectorFlows EjectorFlows =
        Ejector.CalculateFlows(Cycle.Point5, Cycle.Point11);

    [Test]
    public static void TestWrongRefrigerant()
    {
        var refrigerant = new Refrigerant(FluidsList.R407C);
        Action action = () =>
            _ = new VCRCWithEjectorAndRecuperator(
                new Evaporator(refrigerant.Name,
                    Evaporator.Temperature, Evaporator.Superheat),
                Recuperator, Compressor,
                new GasCooler(refrigerant.Name,
                    refrigerant.CriticalTemperature + TemperatureDelta.FromKelvins(5),
                    1.1 * refrigerant.CriticalPressure),
                Ejector);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Refrigerant should not have a temperature glide!*");
    }

    [Test]
    public static void TestWrongTemperatureDifferenceAtRecuperatorHotSide()
    {
        Action action = () =>
            _ = new VCRCWithEjectorAndRecuperator(
                Evaporator,
                new Recuperator(TemperatureDelta.FromKelvins(49)),
                Compressor, GasCooler, Ejector);
        action.Should().Throw<ValidationException>().WithMessage(
            "*Too high temperature difference at recuperator 'hot' side!*");
    }

    [Test]
    public static void TestComponents()
    {
        Cycle.Evaporator.Should().Be(Evaporator);
        Cycle.Recuperator.Should().Be(Recuperator);
        Cycle.Compressor.Should().Be(Compressor);
        Cycle.Condenser.Should().BeNull();
        Cycle.GasCooler.Should().Be(GasCooler);
        Cycle.Ejector.Should().Be(Ejector);
    }

    [Test]
    public static void TestIsTranscritical() =>
        Cycle.IsTranscritical.Should().BeTrue();

    [Test]
    public static void TestPoint1()
    {
        Cycle.Point1.Pressure.Pascals.Should()
            .BeApproximately(Cycle.Point8.Pressure.Pascals, 50);
        Cycle.Point1.Should().Be(Refrigerant.DewPointAt(Cycle.Point1.Pressure));
        Cycle.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint2()
    {
        Cycle.Point2.Should().Be(
            Cycle.Point1.HeatingTo(
                Cycle.Point4.Temperature - Recuperator.TemperatureDifference));
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void TestPoint3s()
    {
        Cycle.Point3s.Should().Be(
            Cycle.Point2.IsentropicCompressionTo(GasCooler.Pressure));
        Cycle.Point3s.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public static void TestPoint3()
    {
        Cycle.Point3.Should().Be(
            Cycle.Point2.CompressionTo(GasCooler.Pressure,
                Compressor.Efficiency));
        Cycle.Point3.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public static void TestPoint4()
    {
        Cycle.Point4.Should().Be(GasCooler.Outlet);
        Cycle.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public static void TestPoint5()
    {
        Cycle.Point5.Should().Be(
            Cycle.Point4.CoolingTo(
                Cycle.Point4.Enthalpy -
                (Cycle.Point2.Enthalpy - Cycle.Point1.Enthalpy)));
        Cycle.Point5.Phase.Should().Be(Phases.SupercriticalLiquid);
    }

    [Test]
    public static void TestPoint6()
    {
        Cycle.Point6.Should().Be(EjectorFlows.NozzleOutlet);
        Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint7()
    {
        Cycle.Point7.Should().Be(EjectorFlows.MixingInlet);
        Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint8()
    {
        Cycle.Point8.Should().Be(EjectorFlows.DiffuserOutlet);
        Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint9()
    {
        Cycle.Point9.Should().Be(Refrigerant.BubblePointAt(Cycle.Point8.Pressure));
        Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint10()
    {
        Cycle.Point10.Should().Be(Cycle.Point9.IsenthalpicExpansionTo(Evaporator.Pressure));
        Cycle.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint11()
    {
        Cycle.Point11.Should().Be(Evaporator.Outlet);
        Cycle.Point11.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint12()
    {
        Cycle.Point12.Should().Be(EjectorFlows.SuctionOutlet);
        Cycle.Point12.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestSpecificMassFlows()
    {
        Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Cycle.HeatReleaserSpecificMassFlow.Percent.Should().BeApproximately(
            (Cycle.EvaporatorSpecificMassFlow *
             (Cycle.Point8.Quality!.Value.DecimalFractions /
              (1 - Cycle.Point8.Quality!.Value.DecimalFractions))).Percent,
            Tolerance);
    }

    [Test]
    public static void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Cycle.Point3s.Enthalpy - Cycle.Point2.Enthalpy));

    [Test]
    public static void TestSpecificWork() =>
        Cycle.SpecificWork.Should().Be(
            Cycle.IsentropicSpecificWork /
            Compressor.Efficiency.DecimalFractions);

    [Test]
    public static void TestSpecificCoolingCapacity() =>
        Cycle.SpecificCoolingCapacity.Should().Be(
            Cycle.Point11.Enthalpy - Cycle.Point10.Enthalpy);

    [Test]
    public static void TestSpecificHeatingCapacity() =>
        Cycle.SpecificHeatingCapacity.Should().Be(
            Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Cycle.Point3.Enthalpy - Cycle.Point4.Enthalpy));

    [Test]
    public static void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            3.22457418804565, Tolerance);
    }

    [Test]
    public static void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            4.224508457719947, Tolerance);
    }

    [Test]
    public static void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(18.82801346274293, Tolerance);

    [Test]
    public static void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(18.82927702845263, Tolerance);

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
            .Should().BeApproximately(30.193479104178696, Tolerance);

    [Test]
    public static void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(1.3462929902114091, Tolerance);

    [Test]
    public static void TestEjectorEnergyLossRatio() =>
        AnalysisResult.EjectorEnergyLossRatio.Percent
            .Should().BeApproximately(9.775280476760091, Tolerance);

    [Test]
    public static void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(15.61478994074089, Tolerance);

    [Test]
    public static void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should().BeApproximately(4.240880459656298, Tolerance);

    [Test]
    public static void TestEconomizerEnergyLossRatio() =>
        AnalysisResult.EconomizerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestMixingEnergyLossRatio() =>
        AnalysisResult.MixingEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestAnalysisRelativeError()
    {
        AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.006710643790451987, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}