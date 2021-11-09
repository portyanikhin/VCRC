using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using VCRC.Fluids;

namespace VCRC.Tests.Fluids
{
    public static class TestRefrigerant
    {
        private static readonly Refrigerant Refrigerant = new(FluidsList.R718);

        [Test]
        public static void TestInitThrows()
        {
            Action action = () => _ = new Refrigerant(FluidsList.Water);
            action.Should().Throw<ValidationException>()
                .WithMessage("*The selected fluid is not a refrigerant (its name should start with 'R')!*");
        }

        [Test(ExpectedResult = 22.064e6)]
        public static double TestCriticalPressure() => Refrigerant.CriticalPressure.Pascals;

        [Test(ExpectedResult = 373.946)]
        public static double TestCriticalTemperature() => Refrigerant.CriticalTemperature.DegreesCelsius;

        [Test(ExpectedResult = 611.65480089686844)]
        public static double TestTriplePressure() => Refrigerant.TriplePressure.Pascals;

        [Test(ExpectedResult = 0.010000000000047748)]
        public static double TestTripleTemperature() => Refrigerant.TripleTemperature.DegreesCelsius;
    }
}