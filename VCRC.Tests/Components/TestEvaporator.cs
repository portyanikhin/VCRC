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
    public static class TestEvaporator
    {
        private const FluidsList RefrigerantName = FluidsList.R407C;
        private static readonly Temperature EvaporatingTemperature = 5.DegreesCelsius();
        private static readonly TemperatureDelta Superheat = TemperatureDelta.FromKelvins(8);

        [TestCase(-74)]
        [TestCase(87)]
        public static void TestWrongTemperature(double temperature)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            Action action = () => _ = new Evaporator(RefrigerantName, temperature.DegreesCelsius(), Superheat);
            action.Should().Throw<ValidationException>()
                .WithMessage("*Evaporating temperature should be in (-73.15;86.2) °C!*");
        }

        [TestCase(-1)]
        [TestCase(51)]
        public static void TestWrongSuperheat(double superheat)
        {
            Action action = () =>
                _ = new Evaporator(RefrigerantName, EvaporatingTemperature, TemperatureDelta.FromKelvins(superheat));
            action.Should().Throw<ValidationException>()
                .WithMessage("*Superheat in the evaporator should be in [0;50] K!*");
        }

        [Test]
        public static void TestPressureDefinition()
        {
            var evaporator = new Evaporator(RefrigerantName, EvaporatingTemperature, Superheat, TwoPhase.Bubble);
            var refrigerant = new Refrigerant(RefrigerantName);
            evaporator.PressureDefinition.Should().Be(TwoPhase.Bubble);
            evaporator.Pressure.Should().Be(refrigerant.WithState(Input.Temperature(EvaporatingTemperature),
                Input.Quality(TwoPhase.Bubble.VaporQuality())).Pressure);
        }
    }
}