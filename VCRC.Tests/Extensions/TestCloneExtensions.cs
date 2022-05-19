using FluentAssertions;
using NUnit.Framework;
using SharpProp;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Tests.Extensions;

public static class TestCloneExtensions
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R718);

    [Test]
    public static void TestClone()
    {
        var origin = Refrigerant.WithState(Input.Pressure(1.Atmospheres()),
            Input.Temperature(20.DegreesCelsius()));
        var clone = origin.Clone();
        clone.Should().Be(origin);
        clone.Update(Input.Pressure(1.Atmospheres()),
            Input.Temperature(30.DegreesCelsius()));
        clone.Should().NotBe(origin);
    }
}