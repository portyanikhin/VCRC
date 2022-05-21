using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests.Components;

public static class TestRecuperator
{
    [TestCase(0)]
    [TestCase(50)]
    public static void TestWrongSuperheat(double superheat)
    {
        Action action = () => _ = new Recuperator(superheat.Kelvins());
        action.Should().Throw<ValidationException>()
            .WithMessage("*Temperature difference at recuperator 'hot' side should be in (0;50) K!*");
    }
}