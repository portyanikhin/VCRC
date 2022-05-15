using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using UnitsNet;
using VCRC.Components;

namespace VCRC.Tests.Components;

public static class TestEconomizer
{
    [TestCase(-1)]
    [TestCase(51)]
    public static void TestWrongSuperheat(double superheat)
    {
        Action action = () =>
            _ = new Economizer(TemperatureDelta.FromKelvins(5),
                TemperatureDelta.FromKelvins(superheat));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Superheat in the economizer should be in [0;50] K!*");
    }
}