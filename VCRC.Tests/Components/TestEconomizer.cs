using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;

namespace VCRC.Tests.Components;

public static class TestEconomizer
{
    private static readonly Evaporator Evaporator =
        new(FluidsList.R32, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Condenser Condenser =
        new(FluidsList.R32, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));

    [TestCase(-1)]
    [TestCase(51)]
    public static void TestWrongSuperheat(double superheat)
    {
        Action action = () =>
            _ = new Economizer(Evaporator, Condenser, TemperatureDelta.FromKelvins(5),
                TemperatureDelta.FromKelvins(superheat));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Superheat in the economizer should be in [0;50] K!*");
    }

    [Test]
    public static void TestEquals()
    {
        var origin =
            new Economizer(Evaporator, Condenser, TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(8));
        var same =
            new Economizer(origin.Pressure, TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(8));
        var other =
            new Economizer(origin.Pressure, TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
        new TestEquals<Economizer>(origin, same, other).Start();
        (origin == same).Should().BeTrue();
        (origin != other).Should().BeTrue();
    }
}