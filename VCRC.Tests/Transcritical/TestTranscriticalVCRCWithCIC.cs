using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Transcritical;

namespace VCRC.Tests.Transcritical;

public class TestTranscriticalVCRCWithCIC
{
    private TranscriticalVCRCWithCIC Cycle { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        const FluidsList refrigerantName = FluidsList.R744;
        var evaporator = new Evaporator(refrigerantName,
            5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var gasCooler = new GasCooler(refrigerantName, 40.DegreesCelsius());
        Cycle = new TranscriticalVCRCWithCIC(evaporator, compressor, gasCooler);
    }

    [Test]
    public void TestWrongRefrigerant()
    {
        const FluidsList refrigerantName = FluidsList.R407C;
        var evaporator = new Evaporator(refrigerantName,
            Cycle.Evaporator.Temperature, Cycle.Evaporator.Superheat);
        var gasCooler = new GasCooler(refrigerantName, 90.DegreesCelsius(),
            new Refrigerant(refrigerantName).CriticalPressure + 1.Bars());
        Action action = () =>
            _ = new TranscriticalVCRCWithCIC(evaporator, Cycle.Compressor, gasCooler);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Refrigerant should not have a temperature glide!*");
    }

    [Test]
    public void TestSpecificMassFlows()
    {
        Cycle.FirstStageSpecificMassFlow.Should().Be(100.Percent());
        Cycle.SecondStageSpecificMassFlow.Percent.Should().BeApproximately(
            (Cycle.FirstStageSpecificMassFlow *
             (1 + (Cycle.Point2.Enthalpy - Cycle.Point3.Enthalpy) /
                 (Cycle.Point3.Enthalpy - Cycle.Point7.Enthalpy)) /
             (1 - Cycle.Point6.Quality!.Value.DecimalFractions)).Percent, 1e-10);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint2s()
    {
        Cycle.Point2s.Pressure.Should().Be(Cycle.IntermediateVessel.Pressure);
        Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
        Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.IntermediateVessel.Pressure);
        Cycle.Point2.Enthalpy.Should().Be(
            Cycle.Point1.Enthalpy + (Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy) /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.IntermediateVessel.Pressure);
        Cycle.Point3.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
        Cycle.Point3.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint4s()
    {
        Cycle.Point4s.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point4s.Entropy.Should().Be(Cycle.Point3.Entropy);
        Cycle.Point4s.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point4.Enthalpy.JoulesPerKilogram.Should().BeApproximately(
            (Cycle.Point3.Enthalpy + (Cycle.Point4s.Enthalpy - Cycle.Point3.Enthalpy) /
                Cycle.Compressor.IsentropicEfficiency.DecimalFractions).JoulesPerKilogram, 1e-10);
        Cycle.Point4.Enthalpy.Should().BeGreaterThan(Cycle.Point4s.Enthalpy);
        Cycle.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point5.Temperature.Should().Be(Cycle.GasCooler.Temperature);
        Cycle.Point5.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint6()
    {
        Cycle.Point6.Pressure.Should().Be(Cycle.IntermediateVessel.Pressure);
        Cycle.Point6.Enthalpy.Should().Be(Cycle.Point5.Enthalpy);
        Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint7()
    {
        Cycle.Point7.Pressure.Should().Be(Cycle.IntermediateVessel.Pressure);
        Cycle.Point7.Quality.Should().Be(TwoPhase.Bubble.VaporQuality());
        Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint8()
    {
        Cycle.Point8.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point8.Enthalpy.Should().Be(Cycle.Point7.Enthalpy);
        Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestEntropyAnalysis()
    {
        var result =
            Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
        const double tolerance = 1e-10;
        result.ThermodynamicPerfection.Percent
            .Should().BeApproximately(16.086268181907716, tolerance);
        result.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(16.017893541004142, tolerance);
        result.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, tolerance);
        result.CondenserEnergyLossRatio.Percent
            .Should().Be(0);
        result.GasCoolerEnergyLossRatio.Percent
            .Should().BeApproximately(13.773659697012475, tolerance);
        result.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(33.119810838209304, tolerance);
        result.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(13.20925639670459, tolerance);
        result.RecuperatorEnergyLossRatio.Percent
            .Should().Be(0);
        result.EconomizerEnergyLossRatio.Percent
            .Should().Be(0);
        result.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(3.8793795270694904, tolerance);
        result.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.42686412372849164, tolerance);
        result.Sum().Percent
            .Should().BeApproximately(100, tolerance);
    }
}