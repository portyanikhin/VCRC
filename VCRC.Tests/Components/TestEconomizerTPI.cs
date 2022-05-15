using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using UnitsNet;
using VCRC.Components;

namespace VCRC.Tests.Components;

public static class TestEconomizerTPI
{
    [TestCase(-1)]
    [TestCase(51)]
    public static void TestWrongTemperatureDifference(double temperatureDifference)
    {
        Action action = () =>
            _ = new EconomizerTPI(TemperatureDelta.FromKelvins(temperatureDifference));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Temperature difference at the economizer 'cold' side should be in [0;50] K!*");
    }
}