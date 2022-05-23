using System;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using UnitsNet.NumberExtensions.NumberToRatio;

namespace VCRC.Tests.Components;

public static class TestEjector
{
    private static readonly Ejector Ejector = new(80.Percent());

    private static readonly Ejector SameEjector = new(80.Percent(), 80.Percent(), 80.Percent());

    [TestCase(0)]
    [TestCase(100)]
    public static void TestWrongIsentropicEfficiency_Single(double isentropicEfficiency)
    {
        Action action = () => _ = new Ejector(isentropicEfficiency.Percent());
        action.Should().Throw<ValidationException>()
            .WithMessage("*Isentropic efficiency of the nozzle should be in (0;100) %!*");
    }

    [TestCase(0, 90, 80, "Isentropic efficiency of the nozzle should be in (0;100) %!")]
    [TestCase(100, 90, 80, "Isentropic efficiency of the nozzle should be in (0;100) %!")]
    [TestCase(90, 0, 80, "Isentropic efficiency of the suction section should be in (0;100) %!")]
    [TestCase(90, 100, 80, "Isentropic efficiency of the suction section should be in (0;100) %!")]
    [TestCase(90, 90, 0, "Isentropic efficiency of the diffuser should be in (0;100) %!")]
    [TestCase(90, 90, 100, "Isentropic efficiency of the diffuser should be in (0;100) %!")]
    public static void TestWrongIsentropicEfficiency_Multiple(
        double nozzleIsentropicEfficiency,
        double suctionIsentropicEfficiency,
        double diffuserIsentropicEfficiency,
        string message)
    {
        Action action = () =>
            _ = new Ejector(
                nozzleIsentropicEfficiency.Percent(),
                suctionIsentropicEfficiency.Percent(),
                diffuserIsentropicEfficiency.Percent());
        action.Should().Throw<ValidationException>()
            .WithMessage($"*{message}*");
    }

    [Test]
    public static void TestEquals() =>
        Ejector.Should().Be(SameEjector);
}