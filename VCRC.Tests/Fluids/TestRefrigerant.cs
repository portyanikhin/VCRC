using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Fluids;

public static class TestRefrigerant
{
    private static readonly Refrigerant Refrigerant =
        new Refrigerant(FluidsList.R718)
            .WithState(Input.Pressure(1.Atmospheres()),
                Input.Temperature(150.DegreesCelsius()));

    private static readonly Pressure HighPressure = 2 * Refrigerant.Pressure;
    private static readonly Pressure LowPressure = 0.5 * Refrigerant.Pressure;
    private static readonly Ratio IsentropicEfficiency = 80.Percent();
    private static readonly TemperatureDelta TemperatureDelta = TemperatureDelta.FromKelvins(10);
    private static readonly SpecificEnergy EnthalpyDelta = 50.KilojoulesPerKilogram();

    [Test]
    public static void TestWrongName()
    {
        Action action = () => _ = new Refrigerant(FluidsList.Water);
        action.Should().Throw<ValidationException>().WithMessage(
            "*The selected fluid is not a refrigerant (its name should start with 'R')!*");
    }

    [Test(ExpectedResult = 22.064e6)]
    public static double TestCriticalPressure() =>
        Refrigerant.CriticalPressure.Pascals;

    [Test(ExpectedResult = 373.946)]
    public static double TestCriticalTemperature() =>
        Refrigerant.CriticalTemperature.DegreesCelsius;

    [Test(ExpectedResult = 611.65480089686844)]
    public static double TestTriplePressure() =>
        Refrigerant.TriplePressure.Pascals;

    [Test(ExpectedResult = 0.010000000000047748)]
    public static double TestTripleTemperature() =>
        Refrigerant.TripleTemperature.DegreesCelsius;

    [TestCase(FluidsList.R22, ExpectedResult = false)]
    [TestCase(FluidsList.R32, ExpectedResult = false)]
    [TestCase(FluidsList.R134a, ExpectedResult = false)]
    [TestCase(FluidsList.R404A, ExpectedResult = true)]
    [TestCase(FluidsList.R407C, ExpectedResult = true)]
    [TestCase(FluidsList.R410A, ExpectedResult = true)]
    public static bool TestHasGlide(FluidsList name) =>
        new Refrigerant(name).HasGlide;

    [Test]
    public static void TestProcesses()
    {
        Refrigerant.IsentropicCompressionTo(HighPressure)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.CompressionTo(HighPressure, IsentropicEfficiency)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.IsenthalpicExpansionTo(LowPressure)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.IsentropicExpansionTo(LowPressure)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.ExpansionTo(LowPressure, IsentropicEfficiency)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.CoolingTo(Refrigerant.Temperature - TemperatureDelta)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.CoolingTo(Refrigerant.Enthalpy - EnthalpyDelta)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.HeatingTo(Refrigerant.Temperature + TemperatureDelta)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.HeatingTo(Refrigerant.Enthalpy + EnthalpyDelta)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.BubblePointAt(1.Atmospheres())
            .Should().BeOfType<Refrigerant>();
        Refrigerant.BubblePointAt(100.DegreesCelsius())
            .Should().BeOfType<Refrigerant>();
        Refrigerant.DewPointAt(1.Atmospheres())
            .Should().BeOfType<Refrigerant>();
        Refrigerant.DewPointAt(100.DegreesCelsius())
            .Should().BeOfType<Refrigerant>();
        Refrigerant.TwoPhasePointAt(1.Atmospheres(), 50.Percent())
            .Should().BeOfType<Refrigerant>();
        Refrigerant.Mixing(
                100.Percent(),
                Refrigerant.CoolingTo(Refrigerant.Temperature - TemperatureDelta),
                200.Percent(),
                Refrigerant.HeatingTo(Refrigerant.Temperature + TemperatureDelta))
            .Should().BeOfType<Refrigerant>();
    }
}