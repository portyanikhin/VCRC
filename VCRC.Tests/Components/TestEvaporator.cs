﻿using System;
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

public static class TestEvaporator
{
    private const FluidsList RefrigerantName = FluidsList.R407C;
    private static readonly Refrigerant Refrigerant = new(RefrigerantName);
    private static readonly Temperature EvaporatingTemperature = 5.DegreesCelsius();
    private static readonly TemperatureDelta Superheat = TemperatureDelta.FromKelvins(8);
    private static readonly Evaporator Evaporator = new(RefrigerantName, EvaporatingTemperature, Superheat);

    [TestCase(-74)]
    [TestCase(87)]
    public static void TestWrongTemperature(double temperature)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () =>
            _ = new Evaporator(RefrigerantName, temperature.DegreesCelsius(), Superheat);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Evaporating temperature should be in (-73.15;86.2) °C!*");
    }

    [TestCase(-1)]
    [TestCase(51)]
    public static void TestWrongSuperheat(double superheat)
    {
        Action action = () =>
            _ = new Evaporator(RefrigerantName, EvaporatingTemperature,
                TemperatureDelta.FromKelvins(superheat));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Superheat in the evaporator should be in [0;50] K!*");
    }

    [Test]
    public static void TestPressureDefinition()
    {
        var evaporator = new Evaporator(RefrigerantName, EvaporatingTemperature,
            Superheat, TwoPhase.Bubble);
        evaporator.PressureDefinition.Should().Be(TwoPhase.Bubble);
        evaporator.Pressure.Should().Be(
            Refrigerant.WithState(Input.Temperature(EvaporatingTemperature),
                Input.Quality(TwoPhase.Bubble.VaporQuality())).Pressure);
    }

    [Test]
    public static void TestDewPoint() =>
        Evaporator.DewPoint.Should().Be(
            Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
                Input.Quality(TwoPhase.Dew.VaporQuality())));

    [Test]
    public static void TestEquals()
    {
        var origin = new Evaporator(RefrigerantName, EvaporatingTemperature, Superheat);
        var same = new Evaporator(RefrigerantName, EvaporatingTemperature, Superheat);
        var other = new Evaporator(RefrigerantName, EvaporatingTemperature,
            Superheat - TemperatureDelta.FromKelvins(3));
        new TestEquals<Evaporator>(origin, same, other).Start();
        (origin == same).Should().BeTrue();
        (origin != other).Should().BeTrue();
    }
}