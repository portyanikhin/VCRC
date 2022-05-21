using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;
using VCRC.Subcritical;

namespace VCRC.Tests.Subcritical;

public class TestVCRCWithParallelCompression
{
    private VCRCWithParallelCompression Cycle { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        const FluidsList refrigerantName = FluidsList.R32;
        var evaporator = new Evaporator(refrigerantName,
            5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(refrigerantName,
            50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));
        Cycle = new VCRCWithParallelCompression(evaporator, compressor, condenser);
    }

    [Test]
    public void TestWrongRefrigerant()
    {
        const FluidsList refrigerantName = FluidsList.R407C;
        var evaporator = new Evaporator(refrigerantName,
            Cycle.Evaporator.Temperature, Cycle.Evaporator.Superheat);
        var condenser = new Condenser(refrigerantName,
            Cycle.Condenser.Temperature, Cycle.Condenser.Subcooling);
        Action action = () =>
            _ = new VCRCWithParallelCompression(evaporator, Cycle.Compressor, condenser);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Refrigerant should not have a temperature glide!*");
    }

    [Test]
    public void TestSpecificMassFlows()
    {
        Cycle.FirstStageSpecificMassFlow.Should().Be(100.Percent());
        Cycle.SecondStageSpecificMassFlow.Should().Be(
            Cycle.FirstStageSpecificMassFlow *
            Cycle.Point7.Quality!.Value.DecimalFractions
            / (1 - Cycle.Point7.Quality!.Value.DecimalFractions));
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint2s()
    {
        Cycle.Point2s.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
        Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.Condenser.Pressure);
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
        Cycle.Point4s.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point4s.Entropy.Should().Be(Cycle.Point3.Entropy);
        Cycle.Point4s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point4.Enthalpy.Should().Be(
            Cycle.Point3.Enthalpy + (Cycle.Point4s.Enthalpy - Cycle.Point3.Enthalpy) /
            Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
        Cycle.Point4.Enthalpy.Should().BeGreaterThan(Cycle.Point4s.Enthalpy);
        Cycle.Point4.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public void TestPoint5()
    {
        Cycle.Point5.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point5.Enthalpy.Should().Be(
            (Cycle.FirstStageSpecificMassFlow.DecimalFractions * Cycle.Point2.Enthalpy +
             Cycle.SecondStageSpecificMassFlow.DecimalFractions * Cycle.Point4.Enthalpy) /
            (Cycle.FirstStageSpecificMassFlow + Cycle.SecondStageSpecificMassFlow).DecimalFractions);
        Cycle.Point5.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Test]
    public void TestPoint6()
    {
        Cycle.Point6.Pressure.Should().Be(Cycle.Condenser.Pressure);
        Cycle.Point6.Temperature.Should().Be(
            Cycle.Condenser.Temperature - Cycle.Condenser.Subcooling);
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
            (Cycle.FirstStageSpecificMassFlow + Cycle.SecondStageSpecificMassFlow).DecimalFractions *
            (Cycle.Point5.Enthalpy - Cycle.Point6.Enthalpy));

    [Test]
    public void TestEntropyAnalysis()
    {
        var result =
            Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
        const double tolerance = 1e-10;
        result.ThermodynamicPerfection.Percent
            .Should().BeApproximately(23.59571035603827, tolerance);
        result.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(23.559922967770717, tolerance);
        result.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, tolerance);
        result.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(27.545779289189316, tolerance);
        result.GasCoolerEnergyLossRatio.Percent
            .Should().Be(0);
        result.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(8.742086452193167, tolerance);
        result.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(19.737899991312165, tolerance);
        result.RecuperatorEnergyLossRatio.Percent
            .Should().Be(0);
        result.EconomizerEnergyLossRatio.Percent
            .Should().Be(0);
        result.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(0.41431129953462587, tolerance);
        result.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.15189942817940816, tolerance);
        result.Sum().Percent
            .Should().BeApproximately(100, tolerance);
    }
}