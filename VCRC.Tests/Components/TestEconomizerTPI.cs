using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests.Components;

public static class TestEconomizerTPI
{
    [TestCase(0)]
    [TestCase(50)]
    public static void TestWrongTemperatureDifference(double temperatureDifference)
    {
        Action action = () =>
            _ = new EconomizerTPI(temperatureDifference.Kelvins());
        action.Should().Throw<ValidationException>().WithMessage(
            "*Temperature difference at the economizer 'cold' side should be in (0;50) K!*");
    }
}