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
            compressor.Should().Be(compressor);
            compressor.Should().BeSameAs(compressor);
            compressor.Should().Be(sameCompressor);
            compressor.Should().NotBeSameAs(sameCompressor);
            compressor.Should().NotBe(otherCompressor);
            compressor.Should().NotBeNull();
            compressor.Equals(new object()).Should().BeFalse();
            (compressor == sameCompressor).Should().Be(compressor.Equals(sameCompressor));
            (compressor != otherCompressor).Should().Be(!compressor.Equals(otherCompressor));
        }
    }
}