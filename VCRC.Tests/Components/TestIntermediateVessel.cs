using System;
using FluentAssertions;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;

namespace VCRC.Tests.Components
{
    public static class TestIntermediateVessel
    {
        private static readonly Evaporator Evaporator =
            new(FluidsList.R32, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

        private static readonly Condenser Condenser =
            new(FluidsList.R32, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));

        [Test]
        public static void TestIntermediatePressure()
        {
            var intermediateVessel = new IntermediateVessel(Evaporator, Condenser);
            intermediateVessel.Pressure.Pascals.Should()
                .Be(Math.Sqrt(Evaporator.Pressure.Pascals * Condenser.Pressure.Pascals));
        }

        [Test]
        public static void TestEquals()
        {
            var intermediateVessel = new IntermediateVessel(Evaporator, Condenser);
            var sameIntermediateVessel = new IntermediateVessel(intermediateVessel.Pressure);
            var otherIntermediateVessel = new IntermediateVessel(intermediateVessel.Pressure + 1.Atmospheres());
            intermediateVessel.Should().Be(intermediateVessel);
            intermediateVessel.Should().BeSameAs(intermediateVessel);
            intermediateVessel.Should().Be(sameIntermediateVessel);
            intermediateVessel.Should().NotBeSameAs(sameIntermediateVessel);
            intermediateVessel.Should().NotBe(otherIntermediateVessel);
            intermediateVessel.Should().NotBeNull();
            intermediateVessel.Equals(new object()).Should().BeFalse();
            (intermediateVessel == sameIntermediateVessel).Should()
                .Be(intermediateVessel.Equals(sameIntermediateVessel));
            (intermediateVessel != otherIntermediateVessel).Should()
                .Be(!intermediateVessel.Equals(otherIntermediateVessel));
        }
    }
}