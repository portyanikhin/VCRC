using System;
using System.Globalization;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Components;

public static class TestEvaporator
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R407C);

    private static readonly Evaporator Evaporator =
        new(Refrigerant.Name, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Evaporator EvaporatorWithoutSuperheat =
        new(Refrigerant.Name, Evaporator.Temperature, TemperatureDelta.Zero);

    private static readonly Refrigerant DewPoint =
        Refrigerant.DewPointAt(Evaporator.Temperature);

    [TestCase(-74)]
    [TestCase(87)]
    public static void TestWrongTemperature(double temperature)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () =>
            _ = new Evaporator(Refrigerant.Name, temperature.DegreesCelsius(),
                Evaporator.Superheat);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Evaporating temperature should be in (-73.15;86.2) °C!*");
    }

    [TestCase(-1)]
    [TestCase(51)]
    public static void TestWrongSuperheat(double superheat)
    {
        Action action = () =>
            _ = new Evaporator(Refrigerant.Name, Evaporator.Temperature,
                TemperatureDelta.FromKelvins(superheat));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Superheat in the evaporator should be in [0;50] K!*");
    }

    [Test]
    public static void TestPressure() =>
        Evaporator.Pressure.Should().Be(Evaporator.Outlet.Pressure);

    [Test]
    public static void TestOutlet()
    {
        Evaporator.Outlet.Should().Be(
            DewPoint.HeatingTo(DewPoint.Temperature + Evaporator.Superheat));
        EvaporatorWithoutSuperheat.Outlet.Should().Be(DewPoint);
    }
}