using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Components;

public static class TestCondenser
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R407C);

    private static readonly Condenser Condenser =
        new(Refrigerant.Name, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));

    [TestCase(-74)]
    [TestCase(87)]
    public static void TestWrongTemperature(double temperature)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () =>
            _ = new Condenser(Refrigerant.Name, temperature.DegreesCelsius(),
                Condenser.Subcooling);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Condensing temperature should be in (-73.15;86.2) °C!*");
    }

    [TestCase(-1)]
    [TestCase(51)]
    public static void TestWrongSubcooling(double subcooling)
    {
        Action action = () =>
            _ = new Condenser(Refrigerant.Name, Condenser.Temperature,
                TemperatureDelta.FromKelvins(subcooling));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Subcooling in the condenser should be in [0;50] K!*");
    }

    [Test]
    public static void TestPressure() =>
        Condenser.Pressure.Should().Be(Condenser.Outlet.Pressure);

    [Test]
    public static void TestOutlet() =>
        Condenser.Outlet.Should().Be(
            Refrigerant.Subcooled(Condenser.Temperature, Condenser.Subcooling));
}