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

public static class TestVCRCWithEjectorEconomizerAndPC
{
    private const double Tolerance = 1e-5;

    private static readonly Refrigerant Refrigerant = new(FluidsList.R32);

    private static readonly Evaporator Evaporator =
        new(Refrigerant.Name, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Compressor Compressor = new(80.Percent());

    private static readonly Condenser Condenser =
        new(Refrigerant.Name, 45.DegreesCelsius(), TemperatureDelta.FromKelvins(3));

    private static readonly Ejector Ejector =
        new(90.Percent(), 90.Percent(), 80.Percent());

    private static readonly Economizer Economizer =
        new(TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));

    private static readonly VCRCWithEjectorEconomizerAndPC Cycle =
        new(Evaporator, Compressor, Condenser, Ejector, Economizer);

    private static readonly EntropyAnalysisResult AnalysisResult =
        Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());

    private static readonly EjectorFlows EjectorFlows =
        Ejector.CalculateFlows(Cycle.Point8, Cycle.Point14);

    [Test]
    public static void TestWrongRefrigerant()
    {
        Action action = () =>
            _ = new VCRCWithEjectorEconomizerAndPC(
                new Evaporator(FluidsList.R407C,
                    Evaporator.Temperature, Evaporator.Superheat),
                Compressor,
                new Condenser(FluidsList.R407C,
                    Condenser.Temperature, Condenser.Subcooling),
                Ejector, Economizer);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Refrigerant should be a single component or an azeotropic blend!*");
    }

    [Test]
    public static void TestWrongEconomizerSuperheat()
    {
        Action action = () =>
            _ = new VCRCWithEjectorEconomizerAndPC(
                Evaporator, Compressor, Condenser, Ejector,
                new Economizer(Economizer.TemperatureDifference,
                    TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at economizer 'hot' side!*");
    }

    [Test]
    public static void TestWrongEconomizerTemperatureDifference()
    {
        Action action = () =>
            _ = new VCRCWithEjectorEconomizerAndPC(
                Evaporator, Compressor, Condenser, Ejector,
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
        Cycle.Ejector.Should().Be(Ejector);
        Cycle.Economizer.Should().Be(Economizer);
    }

    [Test]
    public static void TestIsTranscritical() =>
        Cycle.IsTranscritical.Should().BeFalse();

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
            Cycle.Point1.IsentropicCompressionTo(Condenser.Pressure));
        Cycle.Point2s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint2()
    {
        Cycle.Point2.Should().Be(
            Cycle.Point1.CompressionTo(
                Condenser.Pressure, Compressor.Efficiency));
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public static void TestPoint3()
    {
        Cycle.Point3.Should().Be(
            Refrigerant.Superheated(Cycle.IntermediatePressure, Economizer.Superheat));
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
            Cycle.Point3.CompressionTo(
                Condenser.Pressure, Compressor.Efficiency));
        Cycle.Point4.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint5()
    {
        Cycle.Point5.Should().Be(
            Refrigerant.Mixing(
                Cycle.HeatReleaserSpecificMassFlow - Cycle.IntermediateSpecificMassFlow, Cycle.Point2,
                Cycle.IntermediateSpecificMassFlow, Cycle.Point4));
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
                      Condenser.Pressure.Pascals), 50);

    [Test]
    public static void TestSpecificMassFlows()
    {
        Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Cycle.HeatReleaserSpecificMassFlow.Percent.Should().BeApproximately(
            (Cycle.EvaporatorSpecificMassFlow *
             (Cycle.Point11.Quality!.Value.DecimalFractions /
              (1 - Cycle.Point11.Quality!.Value.DecimalFractions)) *
             (1 + (Cycle.Point6.Enthalpy - Cycle.Point8.Enthalpy) /
                 (Cycle.Point3.Enthalpy - Cycle.Point7.Enthalpy))).Percent,
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
            Cycle.IntermediateSpecificMassFlow.DecimalFractions *
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
            (Cycle.Point5.Enthalpy - Cycle.Point6.Enthalpy));

    [Test]
    public static void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            4.845602639140372, Tolerance);
    }

    [Test]
    public static void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            5.8455164532611485, Tolerance);
    }

    [Test]
    public static void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(28.29306023197195, Tolerance);

    [Test]
    public static void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(28.28640526287078, Tolerance);

    [Test]
    public static void TestCompressorEnergyLossRatio() =>
        AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Tolerance);

    [Test]
    public static void TestCondenserEnergyLossRatio() =>
        AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(21.115461229691114, Tolerance);

    [Test]
    public static void TestGasCoolerEnergyLossRatio() =>
        AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(0.5202571202384826, Tolerance);

    [Test]
    public static void TestEjectorEnergyLossRatio() =>
        AnalysisResult.EjectorEnergyLossRatio.Percent
            .Should().BeApproximately(4.496184544333196, Tolerance);

    [Test]
    public static void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(23.728070141266763, Tolerance);

    [Test]
    public static void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestEconomizerEnergyLossRatio() =>
        AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(1.7980370178155778, Tolerance);

    [Test]
    public static void TestMixingEnergyLossRatio() =>
        AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(0.055584683784087395, Tolerance);

    [Test]
    public static void TestAnalysisRelativeError()
    {
        AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.023527093808216284, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}