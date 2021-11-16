using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;
using VCRC.Components;

namespace VCRC.Tests.Components
{
    public static class TestEconomizer
    {
        [TestCase(-1)]
        [TestCase(51)]
        public static void TestWrongSuperheat(double superheat)
        {
            Action action = () => _ = new Economizer(5.Kelvins(), superheat.Kelvins());
            action.Should().Throw<ValidationException>()
                .WithMessage("*Superheat in the economizer should be in [0;50] K!*");
        }

        [Test]
        public static void TestEquals()
        {
            var economizer = new Economizer(5.Kelvins(), 8.Kelvins());
            var sameEconomizer = new Economizer(5.Kelvins(), 8.Kelvins());
            var otherEconomizer = new Economizer(5.Kelvins(), 5.Kelvins());
            _ = new TestEquals<Economizer>(economizer, sameEconomizer, otherEconomizer);
            (economizer == sameEconomizer).Should().BeTrue();
            (economizer != otherEconomizer).Should().BeTrue();
        }
    }
}