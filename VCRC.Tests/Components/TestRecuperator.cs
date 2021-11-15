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

        [Test]
        public static void TestEquals()
        {
            var recuperator = new Recuperator(5.Kelvins());
            var sameRecuperator = new Recuperator(5.Kelvins());
            var otherRecuperator = new Recuperator(8.Kelvins());
            recuperator.Should().Be(recuperator);
            recuperator.Should().BeSameAs(recuperator);
            recuperator.Should().Be(sameRecuperator);
            recuperator.Should().NotBeSameAs(sameRecuperator);
            recuperator.Should().NotBe(otherRecuperator);
            recuperator.Should().NotBeNull();
            recuperator.Equals(new object()).Should().BeFalse();
            (recuperator == sameRecuperator).Should().Be(recuperator.Equals(sameRecuperator));
            (recuperator != otherRecuperator).Should().Be(!recuperator.Equals(otherRecuperator));
        }
    }
}