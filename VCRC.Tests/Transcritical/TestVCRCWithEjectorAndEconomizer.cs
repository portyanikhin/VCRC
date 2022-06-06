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

public static class TestVCRCWithEjectorAndEconomizer
{
    private const double Tolerance = 1e-7;

    private static readonly Refrigerant Refrigerant = new(FluidsList.R744);

    private static readonly Evaporator Evaporator =
        new(Refrigerant.Name, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Compressor Compressor = new(80.Percent());

    private static readonly GasCooler GasCooler = new(Refrigerant.Name, 40.DegreesCelsius());

    private static readonly Ejector Ejector =
        new(90.Percent(), 90.Percent(), 80.Percent());

    private static readonly Economizer Economizer =
        new(TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));

    private static readonly VCRCWithEjectorAndEconomizer Cycle =
        new(Evaporator, Compressor, GasCooler, Ejector, Economizer);

    private static readonly EntropyAnalysisResult AnalysisResult =
        Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());

    private static readonly EjectorFlows EjectorFlows =
        Ejector.CalculateFlows(Cycle.Point8, Cycle.Point14);

    [Test]
    public static void TestWrongRefrigerant()
    {
        var refrigerant = new Refrigerant(FluidsList.R407C);
        Action action = () =>
            _ = new VCRCWithEjectorAndEconomizer(
                new Evaporator(refrigerant.Name,
                    Evaporator.Temperature, Evaporator.Superheat),
                Compressor,
                new GasCooler(refrigerant.Name,
                    refrigerant.CriticalTemperature + TemperatureDelta.FromKelvins(5),
                    1.1 * refrigerant.CriticalPressure),
                Ejector, Economizer);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Refrigerant should not have a temperature glide!*");
    }

    [Test]
    public static void TestWrongEconomizerSuperheat()
    {
        Action action = () =>
            _ = new VCRCWithEjectorAndEconomizer(
                Evaporator, Compressor, GasCooler, Ejector,
                new Economizer(Economizer.TemperatureDifference,
                    TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at economizer 'hot' side!*");
    }

    [Test]
    public static void TestWrongEconomizerTemperatureDifference()
    {
        Action action = () =>
            _ = new VCRCWithEjectorAndEconomizer(
                Evaporator, Compressor, GasCooler, Ejector,
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
        Cycle.Ejector.Should().Be(Ejector);
        Cycle.Economizer.Should().Be(Economizer);
    }

    [Test]
    public static void TestIsTranscritical() =>
        Cycle.IsTranscritical.Should().BeTrue();

    [Test]
    public static void TestPoint1()
    {
        Cycle.Point1.Should().Be(
            Refrigerant.DewPointAt(EjectorFlows.DiffuserOutlet.Pressure));
        Cycle.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void TestPoint2s()
    {
        Cycle.Point2s.Should().Be(
            Cycle.Point1.IsentropicCompressionTo(Cycle.IntermediatePressure));
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public static void TestPoint2()
    {
        Cycle.Point2.Should().Be(
            Cycle.Point1.CompressionTo(
                Cycle.IntermediatePressure, Compressor.Efficiency));
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public static void TestPoint3()
    {
        Cycle.Point3.Should().Be(
            Refrigerant.Mixing(
                Cycle.HeatReleaserSpecificMassFlow -
                Cycle.IntermediateSpecificMassFlow, Cycle.Point2,
                Cycle.IntermediateSpecificMassFlow, Cycle.Point7));
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
        Cycle.Point5.Should().Be(GasCooler.Outlet);
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
        Cycle.Point7.Should().Be(Refrigerant.Superheated(
            Cycle.IntermediatePressure, Economizer.Superheat));
        Cycle.Point7.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public static void TestPoint8()
    {
        Cycle.Point8.Should().Be(
            Cycle.Point5.CoolingTo(
                Cycle.Point6.Temperature + Economizer.TemperatureDifference));
        Cycle.Point8.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public static void TestPoint9()
    {
        Cycle.Point9.Should().Be(EjectorFlows.NozzleOutlet);
        Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint10()
    {
        Cycle.Point10.Should().Be(EjectorFlows.MixingInlet);
        Cycle.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint11()
    {
        Cycle.Point11.Should().Be(EjectorFlows.DiffuserOutlet);
        Cycle.Point11.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint12()
    {
        Cycle.Point12.Should().Be(
            Refrigerant.BubblePointAt(EjectorFlows.DiffuserOutlet.Pressure));
        Cycle.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint13()
    {
        Cycle.Point13.Should().Be(
            Cycle.Point12.IsenthalpicExpansionTo(Evaporator.Pressure));
        Cycle.Point13.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint14()
    {
        Cycle.Point14.Should().Be(Evaporator.Outlet);
        Cycle.Point14.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint15()
    {
        Cycle.Point15.Should().Be(EjectorFlows.SuctionOutlet);
        Cycle.Point15.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestIntermediatePressure() =>
        Cycle.IntermediatePressure.Pascals.Should().BeApproximately(
            Math.Sqrt(EjectorFlows.DiffuserOutlet.Pressure.Pascals *
                      GasCooler.Pressure.Pascals), 50);

    [Test]
    public static void TestSpecificMassFlows()
    {
        Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Cycle.HeatReleaserSpecificMassFlow.Percent.Should().BeApproximately(
            (Cycle.EvaporatorSpecificMassFlow *
             (Cycle.Point11.Quality!.Value.DecimalFractions /
              (1 - Cycle.Point11.Quality!.Value.DecimalFractions)) *
             (1 + (Cycle.Point5.Enthalpy - Cycle.Point8.Enthalpy) /
                 (Cycle.Point7.Enthalpy - Cycle.Point6.Enthalpy))).Percent,
            Tolerance);
        Cycle.IntermediateSpecificMassFlow.Percent.Should().BeApproximately(
            (Cycle.HeatReleaserSpecificMassFlow - Cycle.EvaporatorSpecificMassFlow *
                (Cycle.Point11.Quality!.Value.DecimalFractions /
                 (1 - Cycle.Point11.Quality!.Value.DecimalFractions))).Percent,
            Tolerance);
    }

    [Test]
    public static void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            (Cycle.HeatReleaserSpecificMassFlow - Cycle.IntermediateSpecificMassFlow).DecimalFractions *
            (Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy) +
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
            Cycle.Point14.Enthalpy - Cycle.Point13.Enthalpy);

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
            3.5076990300399533, Tolerance);
    }

    [Test]
    public static void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            4.50763714253074, Tolerance);
    }

    [Test]
    public static void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(20.48115525010449, Tolerance);

    [Test]
    public static void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(20.44763406769744, Tolerance);

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
            .Should().BeApproximately(21.62366337364647, Tolerance);

    [Test]
    public static void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(6.0218512390103545, Tolerance);

    [Test]
    public static void TestEjectorEnergyLossRatio() =>
        AnalysisResult.EjectorEnergyLossRatio.Percent
            .Should().BeApproximately(12.556916295285012, Tolerance);

    [Test]
    public static void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(16.953384710916637, Tolerance);

    [Test]
    public static void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestEconomizerEnergyLossRatio() =>
        AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(2.199955238320946, Tolerance);

    [Test]
    public static void TestMixingEnergyLossRatio() =>
        AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(0.19659507512313654, Tolerance);

    [Test]
    public static void TestAnalysisRelativeError()
    {
        AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.16393672879741025, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}