using System;
using System.Globalization;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Tests.Components
{
    public static class TestCondenser
    {
        private const FluidsList RefrigerantName = FluidsList.R407C;
        private static readonly Temperature CondensingTemperature = 50.DegreesCelsius();
        private static readonly TemperatureDelta Subcooling = TemperatureDelta.FromKelvins(3);

        [TestCase(-74)]
        [TestCase(87)]
        public static void TestWrongTemperature(double temperature)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            Action action = () => _ = new Condenser(RefrigerantName, temperature.DegreesCelsius(), Subcooling);
            action.Should().Throw<ValidationException>()
                .WithMessage("*Condensing temperature should be in (-73.15;86.2) °C!*");
        }

        [TestCase(-1)]
        [TestCase(51)]
        public static void TestWrongSubcooling(double subcooling)
        {
            Action action = () =>
                _ = new Condenser(RefrigerantName, CondensingTemperature, TemperatureDelta.FromKelvins(subcooling));
            action.Should().Throw<ValidationException>()
                .WithMessage("*Subcooling in the condenser should be in [0;50] K!*");
        }

        [Test]
        public static void TestPressureDefinition()
        {
            var condenser = new Condenser(RefrigerantName, CondensingTemperature, Subcooling, TwoPhase.Dew);
            var refrigerant = new Refrigerant(RefrigerantName);
            condenser.PressureDefinition.Should().Be(TwoPhase.Dew);
            condenser.Pressure.Should().Be(refrigerant.WithState(Input.Temperature(CondensingTemperature),
                Input.Quality(TwoPhase.Dew.VaporQuality())).Pressure);
        }
        
        [Test]
        public static void TestEquals()
        {
            var condenser = new Condenser(RefrigerantName, CondensingTemperature, Subcooling);
            var sameCondenser = new Condenser(RefrigerantName, CondensingTemperature, Subcooling);
            var otherCondenser = new Condenser(RefrigerantName, CondensingTemperature,
                Subcooling + TemperatureDelta.FromKelvins(2));
            condenser.Should().Be(condenser);
            condenser.Should().BeSameAs(condenser);
            condenser.Should().Be(sameCondenser);
            condenser.Should().NotBeSameAs(sameCondenser);
            condenser.Should().NotBe(otherCondenser);
            condenser.Should().NotBeNull();
            condenser.Equals(new object()).Should().BeFalse();
            (condenser == sameCondenser).Should().Be(condenser.Equals(sameCondenser));
            (condenser != otherCondenser).Should().Be(!condenser.Equals(otherCondenser));
        }
    }
}