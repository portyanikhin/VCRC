﻿using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Subcritical;

public class TestVCRCWithParallelCompression
{
    private const double Tolerance = 1e-10;
    private VCRCWithParallelCompression Cycle { get; set; } = null!;
    private EntropyAnalysisResult AnalysisResult { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        var evaporator = new Evaporator(FluidsList.R32, 5.DegreesCelsius(),
            TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(FluidsList.R32, 45.DegreesCelsius(),
            TemperatureDelta.FromKelvins(3));
        Cycle = new VCRCWithParallelCompression(evaporator, compressor, condenser);
        AnalysisResult = Cycle.EntropyAnalysis(
            18.DegreesCelsius(), 35.DegreesCelsius());
    }

    [Test]
    public void TestWrongRefrigerant()
    {
        Action action = () =>
            _ = new VCRCWithParallelCompression(
                new Evaporator(FluidsList.R407C,
                    Cycle.Evaporator.Temperature, Cycle.Evaporator.Superheat),
                Cycle.Compressor,
                new Condenser(FluidsList.R407C,
                    Cycle.Condenser!.Temperature, Cycle.Condenser!.Subcooling));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Refrigerant should not have a temperature glide!*");
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
        Cycle.Point2s.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
        Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point2.Enthalpy.Should().Be(
            Cycle.Point1.Enthalpy + (Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy) /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point3.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
        Cycle.Point3.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint4s()
    {
        Cycle.Point4s.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point4s.Entropy.Should().Be(Cycle.Point3.Entropy);
        Cycle.Point4s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point4.Enthalpy.KilojoulesPerKilogram.Should().BeApproximately(
            (Cycle.Point3.Enthalpy + (Cycle.Point4s.Enthalpy - Cycle.Point3.Enthalpy) /
                Cycle.Compressor.IsentropicEfficiency.DecimalFractions).KilojoulesPerKilogram,
            Tolerance);
        Cycle.Point4.Enthalpy.Should().BeGreaterThan(Cycle.Point4s.Enthalpy);
        Cycle.Point4.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point5.Enthalpy.Should().Be(
            (Cycle.EvaporatorSpecificMassFlow.DecimalFractions * Cycle.Point2.Enthalpy +
             (Cycle.HeatReleaserSpecificMassFlow - Cycle.EvaporatorSpecificMassFlow).DecimalFractions
             * Cycle.Point4.Enthalpy) / Cycle.HeatReleaserSpecificMassFlow.DecimalFractions);
        Cycle.Point5.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint6()
    {
        Cycle.Point6.Pressure.Should().Be(Cycle.Condenser!.Pressure);
        Cycle.Point6.Temperature.Should().Be(
            Cycle.Condenser!.Temperature - Cycle.Condenser!.Subcooling);
        Cycle.Point6.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public void TestPoint7()
    {
        Cycle.Point7.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point7.Enthalpy.Should().Be(Cycle.Point6.Enthalpy);
        Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestPoint8()
    {
        Cycle.Point8.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point8.Quality.Should().Be(TwoPhase.Bubble.VaporQuality());
        Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
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
        Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Cycle.HeatReleaserSpecificMassFlow.Should().Be(
            Cycle.EvaporatorSpecificMassFlow *
            (1 + Cycle.Point7.Quality!.Value.DecimalFractions /
                (1 - Cycle.Point7.Quality!.Value.DecimalFractions)));
    }

    [Test]
    public void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy +
            (Cycle.HeatReleaserSpecificMassFlow - Cycle.EvaporatorSpecificMassFlow).DecimalFractions *
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
            Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Cycle.Point5.Enthalpy - Cycle.Point6.Enthalpy));

    [Test]
    public void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            4.652194564254316, Tolerance);
    }

    [Test]
    public void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            5.652194564254314, Tolerance);
    }

    [Test]
    public void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(27.163766990322298, Tolerance);

    [Test]
    public void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(27.13040697950264, Tolerance);

    [Test]
    public void TestCompressorEnergyLossRatio() =>
        AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Tolerance);

    [Test]
    public void TestCondenserEnergyLossRatio() =>
        AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(22.008885324896358, Tolerance);

    [Test]
    public void TestGasCoolerEnergyLossRatio() =>
        AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(7.7851111316038475, Tolerance);

    [Test]
    public void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(22.73292679407433, Tolerance);

    [Test]
    public void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestEconomizerEnergyLossRatio() =>
        AnalysisResult.EconomizerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public void TestMixingEnergyLossRatio() =>
        AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(0.3426697699228131, Tolerance);

    [Test]
    public void TestAnalysisRelativeError()
    {
        AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.12296170435208868, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}