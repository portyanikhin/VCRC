using System;
using System.Globalization;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToTemperature;
using UnitsNet.Units;
using VCRC.Components;

namespace VCRC.Tests.Components;

public static class TestGasCooler
{
    private const FluidsList RefrigerantName = FluidsList.R744;
    private static readonly Temperature OutletTemperature = 35.DegreesCelsius();
    private static readonly GasCooler GasCooler = new(RefrigerantName, OutletTemperature);

    [TestCase(30)]
    [TestCase(1727)]
    public static void TestWrongTemperature(double temperature)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () => _ = new GasCooler(RefrigerantName, temperature.DegreesCelsius());
        action.Should().Throw<ValidationException>()
            .WithMessage("*Gas cooler outlet temperature should be in (30.98;1726.85) °C!*");
    }

    [Test]
    public static void TestNeedToDefinePressure()
    {
        Action action = () => _ = new GasCooler(FluidsList.R729, OutletTemperature);
        action.Should().Throw<ArgumentException>()
            .WithMessage(
                "It is impossible to automatically calculate the absolute pressure in the gas cooler! " +
                "It is necessary to define it.");
    }

    [Test]
    public static void TestPressure() =>
        GasCooler.Pressure.Should().Be((2.759 * OutletTemperature.DegreesCelsius - 9.912).Bars());

    [Test]
    public static void TestEquals()
    {
        var gasCooler = new GasCooler(RefrigerantName, OutletTemperature);
        var sameGasCooler = new GasCooler(RefrigerantName, OutletTemperature,
            (2.759 * OutletTemperature.DegreesCelsius - 9.912).Bars().ToUnit(PressureUnit.Kilopascal));
        var otherGasCooler = new GasCooler(RefrigerantName, OutletTemperature + TemperatureDelta.FromKelvins(2));
        _ = new TestEquals<GasCooler>(gasCooler, sameGasCooler, otherGasCooler);
        (gasCooler == sameGasCooler).Should().BeTrue();
        (gasCooler != otherGasCooler).Should().BeTrue();
    }
}