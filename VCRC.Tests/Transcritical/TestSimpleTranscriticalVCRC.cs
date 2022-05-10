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
using VCRC.Transcritical;

namespace VCRC.Tests.Transcritical;

public class TestSimpleTranscriticalVCRC
{
    private SimpleTranscriticalVCRC Cycle { get; set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        const FluidsList refrigerantName = FluidsList.R744;
        var evaporator = new Evaporator(refrigerantName, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
        var compressor = new Compressor(80.Percent());
        var gasCooler = new GasCooler(refrigerantName, 40.DegreesCelsius());
        Cycle = new SimpleTranscriticalVCRC(evaporator, compressor, gasCooler);
    }

    [Test]
    public void TestWrongRefrigerantNames()
    {
        Action action = () =>
            _ = new SimpleTranscriticalVCRC(
                new Evaporator(FluidsList.R32, Cycle.Evaporator.Temperature, Cycle.Evaporator.Superheat),
                Cycle.Compressor,
                new GasCooler(FluidsList.R744, Cycle.GasCooler.Temperature));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Only one refrigerant should be selected!*");
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void TestPoint2s()
    {
        Cycle.Point2s.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
        Cycle.Point2s.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint2()
    {
        Cycle.Point2.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point2.Enthalpy.Should().Be(Cycle.Point1.Enthalpy + Cycle.SpecificWork);
        Cycle.Point2.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint3()
    {
        Cycle.Point3.Pressure.Should().Be(Cycle.GasCooler.Pressure);
        Cycle.Point3.Temperature.Should().Be(Cycle.GasCooler.Temperature);
        Cycle.Point3.Phase.Should().Be(Phases.Supercritical);
    }

    [Test]
    public void TestPoint4()
    {
        Cycle.Point4.Pressure.Should().Be(Cycle.Evaporator.Pressure);
        Cycle.Point4.Enthalpy.Should().Be(Cycle.Point3.Enthalpy);
        Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public void TestEntropyAnalysis()
    {
        var result = Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
        const double tolerance = 1e-10;
        result.ThermodynamicPerfection.Percent.Should().BeApproximately(15.324303610656681, tolerance);
        result.MinSpecificWorkRatio.Percent.Should().BeApproximately(15.324303610656681, tolerance);
        result.CompressorEnergyLossRatio.Percent.Should().BeApproximately(20, tolerance);
        result.CondenserEnergyLossRatio.Percent.Should().Be(0);
        result.GasCoolerEnergyLossRatio.Percent.Should().BeApproximately(21.942420064235975, tolerance);
        result.ExpansionValvesEnergyLossRatio.Percent.Should().BeApproximately(30.20483902058083, tolerance);
        result.EvaporatorEnergyLossRatio.Percent.Should().BeApproximately(12.528437304526513, tolerance);
        result.RecuperatorEnergyLossRatio.Percent.Should().Be(0);
        result.EconomizerEnergyLossRatio.Percent.Should().Be(0);
        result.MixingEnergyLossRatio.Percent.Should().Be(0);
        result.AnalysisRelativeError.Percent.Should().BeApproximately(0, tolerance);
        result.Sum().Percent.Should().BeApproximately(100, tolerance);
    }
}