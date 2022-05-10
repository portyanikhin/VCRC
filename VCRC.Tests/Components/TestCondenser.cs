using System;
using System.Globalization;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Tests.Components;

public static class TestCondenser
{
    private static readonly Condenser Condenser =
        new(FluidsList.R407C, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));

    [TestCase(-74)]
    [TestCase(87)]
    public static void TestWrongTemperature(double temperature)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () =>
            _ = new Condenser(Condenser.RefrigerantName, temperature.DegreesCelsius(),
                Condenser.Subcooling);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Condensing temperature should be in (-73.15;86.2) °C!*");
    }

    [TestCase(-1)]
    [TestCase(51)]
    public static void TestWrongSubcooling(double subcooling)
    {
        Action action = () =>
            _ = new Condenser(Condenser.RefrigerantName, Condenser.Temperature,
                TemperatureDelta.FromKelvins(subcooling));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Subcooling in the condenser should be in [0;50] K!*");
    }

    [Test]
    public static void TestPressure() =>
        Condenser.Pressure.Should().Be(
            new Refrigerant(Condenser.RefrigerantName)
                .WithState(Input.Temperature(Condenser.Temperature),
                    Input.Quality(TwoPhase.Bubble.VaporQuality())).Pressure);
}