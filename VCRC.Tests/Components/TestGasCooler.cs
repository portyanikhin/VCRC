using System;
using System.Globalization;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;

namespace VCRC.Tests.Components;

public static class TestGasCooler
{
    private static readonly GasCooler GasCooler =
        new(FluidsList.R744, 35.DegreesCelsius());

    [Test]
    public static void TestNeedToDefinePressure()
    {
        Action action = () =>
            _ = new GasCooler(FluidsList.R729, GasCooler.Temperature);
        action.Should().Throw<ArgumentException>()
            .WithMessage(
                "It is impossible to automatically calculate the absolute pressure in the gas cooler! " +
                "It is necessary to define it.");
    }

    [Test]
    public static void TestWrongTemperature()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () =>
            _ = new GasCooler(GasCooler.RefrigerantName, 30.DegreesCelsius());
        action.Should().Throw<ValidationException>()
            .WithMessage("*Gas cooler outlet temperature should be greater than 30.98 °C!*");
    }

    [Test]
    public static void TestWrongPressure()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () =>
            _ = new GasCooler(GasCooler.RefrigerantName, GasCooler.Temperature, Pressure.Zero);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Gas cooler absolute pressure should be greater than 7.38 MPa!*");
    }

    [Test]
    public static void TestPressure() =>
        GasCooler.Pressure.Should().Be(
            (2.759 * GasCooler.Temperature.DegreesCelsius - 9.912).Bars());
}