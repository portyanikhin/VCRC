using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class CondenserTests
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R407C);
    private static readonly Temperature Temperature = 323.15.Kelvins();
    private static readonly TemperatureDelta Subcooling = TemperatureDelta.FromDegreesCelsius(3);

    public CondenserTests() =>
        Condenser = new Condenser(Refrigerant.Name, Temperature, Subcooling);

    private Condenser Condenser { get; }

    [Theory]
    [InlineData(-74.0)]
    [InlineData(87.0)]
    public void Condenser_WrongTemperature_ThrowsValidationException(double temperature)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () => _ = new Condenser(
            Refrigerant.Name, temperature.DegreesCelsius(), Subcooling);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Condensing temperature should be in (-73.15;86.2) °C!*");
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(51.0)]
    public void Condenser_WrongSubcooling_ThrowsValidationException(double subcooling)
    {
        Action action = () => _ = new Condenser(
            Refrigerant.Name, Temperature, TemperatureDelta.FromKelvins(subcooling));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Subcooling in the condenser should be in [0;50] K!*");
    }

    [Fact]
    public void RefrigerantName_Always_ReturnsEnteredName() =>
        Condenser.RefrigerantName.Should().Be(Refrigerant.Name);

    [Fact]
    public void Temperature_Always_ReturnsEnteredValueInCelsius()
    {
        Condenser.Temperature.Should().Be(Temperature);
        Condenser.Temperature.Unit.Should().Be(TemperatureUnit.DegreeCelsius);
    }

    [Fact]
    public void Subcooling_Always_ReturnsEnteredValueInKelvins()
    {
        Condenser.Subcooling.Should().Be(Subcooling);
        Condenser.Subcooling.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }

    [Fact]
    public void Pressure_Always_ReturnsOutletPressureInKilopascals()
    {
        Condenser.Pressure.Should().Be(Condenser.Outlet.Pressure);
        Condenser.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Outlet_Always_ReturnsSubcooledRefrigerant() =>
        Condenser.Outlet.Should().Be(Refrigerant.Subcooled(Temperature, Subcooling));
}