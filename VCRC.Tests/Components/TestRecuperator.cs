using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;
using VCRC.Components;

namespace VCRC.Tests.Components
{
    public static class TestRecuperator
    {
        [TestCase(-1)]
        [TestCase(51)]
        public static void TestWrongSuperheat(double superheat)
        {
            Action action = () => _ = new Recuperator(superheat.Kelvins());
            action.Should().Throw<ValidationException>()
                .WithMessage("*Superheat in the recuperator should be in [0;50] K!*");
        }
    }
}