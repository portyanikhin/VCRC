using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.NumberExtensions.NumberToSpeed;
using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Subcritical;

public static class TestVCRCWithEjector
{
    private const double Tolerance = 1e-8;

    private static readonly Refrigerant Refrigerant = new(FluidsList.R32);

    private static readonly Evaporator Evaporator =
        new(Refrigerant.Name, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Compressor Compressor = new(80.Percent());

    private static readonly Condenser Condenser =
        new(Refrigerant.Name, 45.DegreesCelsius(), TemperatureDelta.FromKelvins(3));

    private static readonly Ejector Ejector =
        new(90.Percent(), 90.Percent(), 80.Percent());

    private static readonly VCRCWithEjector Cycle =
        new(Evaporator, Compressor, Condenser, Ejector);

    private static readonly EntropyAnalysisResult AnalysisResult =
        Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());

    private static readonly Ratio EjectorFlowRatio =
        Cycle.Point6.Quality!.Value;

    private static readonly Speed Point5Speed =
        EjectorFlowRatio.DecimalFractions *
        CalculateOutletSpeed(Cycle.Point3, Cycle.Point4) +
        (1 - EjectorFlowRatio.DecimalFractions) *
        CalculateOutletSpeed(Cycle.Point9, Cycle.Point10);

    private static readonly SpecificEnergy Point5KineticEnergy =
        (Math.Pow(Point5Speed.MetersPerSecond, 2) / 2.0)
        .JoulesPerKilogram();

    [Test]
    public static void TestComponents()
    {
        Cycle.Evaporator.Should().Be(Evaporator);
        Cycle.Compressor.Should().Be(Compressor);
        Cycle.Condenser.Should().Be(Condenser);
        Cycle.GasCooler.Should().BeNull();
        Cycle.Ejector.Should().Be(Ejector);
    }

    [Test]
    public static void TestIsTranscritical() =>
        Cycle.IsTranscritical.Should().BeFalse();

    [Test]
    public static void TestPoint1()
    {
        Cycle.Point1.Should().Be(
            Refrigerant.DewPointAt(Cycle.EjectorDiffuserPressure));
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
            Cycle.Point1.CompressionTo(Condenser.Pressure,
                Compressor.Efficiency));
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public static void TestPoint3()
    {
        Cycle.Point3.Should().Be(
            Refrigerant.BubblePointAt(Condenser.Temperature)
                .CoolingTo(Condenser.Temperature - Condenser.Subcooling));
        Cycle.Point3.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public static void TestPoint4()
    {
        Cycle.Point4.Should().Be(
            Cycle.Point3.ExpansionTo(Cycle.EjectorMixingPressure,
                Ejector.NozzleEfficiency));
        Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.EjectorMixingPressure);
        Cycle.Point5.Enthalpy.JoulesPerKilogram.Should().BeApproximately(
            (EjectorFlowRatio.DecimalFractions * Cycle.Point3.Enthalpy +
             (1 - EjectorFlowRatio.DecimalFractions) * Cycle.Point9.Enthalpy -
             Point5KineticEnergy).JoulesPerKilogram, 10);
        Cycle.Point5.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint6()
    {
        Cycle.Point6.Pressure.Should().Be(Cycle.EjectorDiffuserPressure);
        Cycle.Point6.Enthalpy.JoulesPerKilogram.Should().BeApproximately(
            (Cycle.Point5.Enthalpy + Point5KineticEnergy).JoulesPerKilogram, 10);
        Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint7()
    {
        Cycle.Point7.Should().Be(
            Refrigerant.BubblePointAt(Cycle.EjectorDiffuserPressure));
        Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint8()
    {
        Cycle.Point8.Should().Be(
            Cycle.Point7.IsenthalpicExpansionTo(Evaporator.Pressure));
        Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint9()
    {
        Cycle.Point9.Should().Be(
            Refrigerant.DewPointAt(Evaporator.Temperature)
                .HeatingTo(Evaporator.Temperature + Evaporator.Superheat));
        Cycle.Point9.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint10()
    {
        Cycle.Point10.Should().Be(
            Cycle.Point9.ExpansionTo(Cycle.EjectorMixingPressure,
                Ejector.SuctionEfficiency));
        Cycle.Point10.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestEjectorMixingPressure() =>
        Cycle.EjectorMixingPressure.Should().Be(0.9 * Evaporator.Pressure);

    [Test]
    public static void TestEjectorDiffuserPressure() =>
        Cycle.EjectorDiffuserPressure.Pascals.Should().BeApproximately(
            Refrigerant.WithState(Input.Entropy(Cycle.Point5.Entropy),
                    Input.Enthalpy(
                        Cycle.Point5.Enthalpy +
                        Ejector.DiffuserEfficiency.DecimalFractions * Point5KineticEnergy))
                .Pressure.Pascals, 100);

    [Test]
    public static void TestSpecificMassFlows()
    {
        Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Cycle.HeatReleaserSpecificMassFlow.Percent.Should().BeApproximately(
            (Cycle.EvaporatorSpecificMassFlow *
             (Cycle.Point6.Quality!.Value.DecimalFractions /
              (1 - Cycle.Point6.Quality!.Value.DecimalFractions))).Percent,
            Tolerance);
    }

    [Test]
    public static void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy));

    [Test]
    public static void TestSpecificWork() =>
        Cycle.SpecificWork.Should().Be(
            Cycle.IsentropicSpecificWork /
            Compressor.Efficiency.DecimalFractions);

    [Test]
    public static void TestSpecificCoolingCapacity() =>
        Cycle.SpecificCoolingCapacity.Should().Be(
            Cycle.Point9.Enthalpy - Cycle.Point8.Enthalpy);

    [Test]
    public static void TestSpecificHeatingCapacity() =>
        Cycle.SpecificHeatingCapacity.Should().Be(
            Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Cycle.Point2.Enthalpy - Cycle.Point3.Enthalpy));

    [Test]
    public static void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            4.836643336835533, Tolerance);
    }

    [Test]
    public static void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            5.836563370628909, Tolerance);
    }

    [Test]
    public static void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(28.240747630501133, Tolerance);

    [Test]
    public static void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(28.24357079452348, Tolerance);

    [Test]
    public static void TestCompressorEnergyLossRatio() =>
        AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Tolerance);

    [Test]
    public static void TestCondenserEnergyLossRatio() =>
        AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(21.00249583519198, Tolerance);

    [Test]
    public static void TestGasCoolerEnergyLossRatio() =>
        AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(0.19078569109720636, Tolerance);

    [Test]
    public static void TestEjectorEnergyLossRatio() =>
        AnalysisResult.EjectorEnergyLossRatio.Percent
            .Should().BeApproximately(6.8730525745277244, Tolerance);

    [Test]
    public static void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(23.69009510465962, Tolerance);

    [Test]
    public static void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

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
            .Should().BeApproximately(0.009995775827658926, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }

    private static Speed CalculateOutletSpeed(AbstractFluid inlet, AbstractFluid outlet) =>
        Math.Sqrt(2 * (inlet.Enthalpy - outlet.Enthalpy).JoulesPerKilogram).MetersPerSecond();
}