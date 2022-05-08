using System;
using FluentAssertions;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
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
    public static void TestIntermediatePressure()
    {
        var intermediateVessel = new IntermediateVessel(Evaporator, Condenser);
        intermediateVessel.Pressure.Pascals.Should()
            .Be(Math.Sqrt(Evaporator.Pressure.Pascals * Condenser.Pressure.Pascals));
    }

    [Test]
    public static void TestEquals()
    {
        var origin = new IntermediateVessel(Evaporator, Condenser);
        var same = new IntermediateVessel(origin.Pressure);
        var other = new IntermediateVessel(origin.Pressure + 1.Atmospheres());
        new TestEquals<IntermediateVessel>(origin, same, other).Start();
        (origin == same).Should().BeTrue();
        (origin != other).Should().BeTrue();
    }
}