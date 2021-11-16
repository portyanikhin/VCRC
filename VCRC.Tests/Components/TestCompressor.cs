using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using UnitsNet.NumberExtensions.NumberToRatio;
using VCRC.Components;

namespace VCRC.Tests.Components
{
    public static class TestCompressor
    {
        [TestCase(0)]
        [TestCase(100)]
        public static void TestWrongIsentropicEfficiency(double isentropicEfficiency)
        {
            Action action = () => _ = new Compressor(isentropicEfficiency.Percent());
            action.Should().Throw<ValidationException>()
                .WithMessage("*Isentropic efficiency of the compressor should be in (0;100) %!*");
        }

        [Test]
        public static void TestEquals()
        {
            var compressor = new Compressor(80.Percent());
            var sameCompressor = new Compressor(80.Percent());
            var otherCompressor = new Compressor(70.Percent());
            _ = new TestEquals<Compressor>(compressor, sameCompressor, otherCompressor);
            (compressor == sameCompressor).Should().BeTrue();
            (compressor != otherCompressor).Should().BeTrue();
        }
    }
}