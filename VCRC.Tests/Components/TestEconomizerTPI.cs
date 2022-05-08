using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;

namespace VCRC.Tests.Components;

public static class TestEconomizerTPI
{
    private static readonly Evaporator Evaporator =
        new(FluidsList.R32, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Condenser Condenser =
        new(FluidsList.R32, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));

    [TestCase(-1)]
    [TestCase(51)]
    public static void TestWrongTemperatureDifference(double temperatureDifference)
    {
        Action action = () =>
            _ = new EconomizerTPI(Evaporator, Condenser,
                TemperatureDelta.FromKelvins(temperatureDifference));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Temperature difference at the economizer 'cold' side should be in [0;50] K!*");
    }

    [Test]
    public static void TestIntermediatePressure()
    {
        var economizer = new EconomizerTPI(Evaporator, Condenser, TemperatureDelta.FromKelvins(5));
        economizer.Pressure.Pascals.Should()
            .Be(Math.Sqrt(Evaporator.Pressure.Pascals * Condenser.Pressure.Pascals));
    }

    [Test]
    public static void TestEquals()
    {
        var origin = new EconomizerTPI(Evaporator, Condenser, TemperatureDelta.FromKelvins(5));
        var same =
            new EconomizerTPI(origin.Pressure, TemperatureDelta.FromKelvins(5));
        var other =
            new EconomizerTPI(origin.Pressure + 1.Atmospheres(),
                TemperatureDelta.FromKelvins(10));
        new TestEquals<EconomizerTPI>(origin, same, other).Start();
        (origin == same).Should().BeTrue();
        (origin != other).Should().BeTrue();
    }
}