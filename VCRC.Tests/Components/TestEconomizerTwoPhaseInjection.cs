using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;

namespace VCRC.Tests.Components
{
    public static class TestEconomizerTwoPhaseInjection
    {
        private static readonly Evaporator Evaporator =
            new(FluidsList.R32, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

        private static readonly Condenser Condenser =
            new(FluidsList.R32, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));
        
        [TestCase(-1)]
        [TestCase(21)]
        public static void TestWrongTemperatureDifference(double temperatureDifference)
        {
            Action action = () =>
                _ = new EconomizerTwoPhaseInjection(Evaporator, Condenser,
                    TemperatureDelta.FromKelvins(temperatureDifference));
            action.Should().Throw<ValidationException>()
                .WithMessage("*Temperature difference at the economizer 'cold' side should be in [0;20] K!*");
        }
        
        [Test]
        public static void TestIntermediatePressure()
        {
            var economizer = new EconomizerTwoPhaseInjection(Evaporator, Condenser, TemperatureDelta.FromKelvins(5));
            economizer.Pressure.Pascals.Should()
                .Be(Math.Sqrt(Evaporator.Pressure.Pascals * Condenser.Pressure.Pascals));
        }
        
        [Test]
        public static void TestEquals()
        {
            var economizer = new EconomizerTwoPhaseInjection(Evaporator, Condenser, TemperatureDelta.FromKelvins(5));
            var sameEconomizer =
                new EconomizerTwoPhaseInjection(economizer.Pressure, TemperatureDelta.FromKelvins(5));
            var otherEconomizer =
                new EconomizerTwoPhaseInjection(economizer.Pressure + 1.Atmospheres(),
                    TemperatureDelta.FromKelvins(10));
            _ = new TestEquals<EconomizerTwoPhaseInjection>(economizer, sameEconomizer, otherEconomizer);
            (economizer == sameEconomizer).Should().BeTrue();
            (economizer != otherEconomizer).Should().BeTrue();
        }
    }
}