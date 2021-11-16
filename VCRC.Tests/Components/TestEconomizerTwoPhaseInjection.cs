using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;
using VCRC.Components;

namespace VCRC.Tests.Components
{
    public static class TestEconomizerTwoPhaseInjection
    {
        [TestCase(-1)]
        [TestCase(21)]
        public static void TestWrongTemperatureDifference(double temperatureDifference)
        {
            Action action = () => _ = new EconomizerTwoPhaseInjection(temperatureDifference.Kelvins());
            action.Should().Throw<ValidationException>()
                .WithMessage("*Temperature difference at the economizer 'cold' side should be in [0;20] K!*");
        }
        
        [Test]
        public static void TestEquals()
        {
            var economizer = new EconomizerTwoPhaseInjection(5.Kelvins());
            var sameEconomizer = new EconomizerTwoPhaseInjection(5.Kelvins());
            var otherEconomizer = new EconomizerTwoPhaseInjection(10.Kelvins());
            _ = new TestEquals<EconomizerTwoPhaseInjection>(economizer, sameEconomizer, otherEconomizer);
            (economizer == sameEconomizer).Should().BeTrue();
            (economizer != otherEconomizer).Should().BeTrue();
        }
    }
}