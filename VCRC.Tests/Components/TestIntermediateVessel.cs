using System;
using FluentAssertions;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;

namespace VCRC.Tests.Components;

public static class TestIntermediateVessel
{
    private static readonly Evaporator Evaporator =
        new(FluidsList.R32, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Condenser Condenser =
        new(FluidsList.R32, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));

    [Test]
    public static void TestPressure() =>
        new IntermediateVessel(Evaporator, Condenser).Pressure.Pascals
            .Should().Be(Math.Sqrt(Evaporator.Pressure.Pascals * Condenser.Pressure.Pascals));
}