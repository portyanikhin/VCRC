using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class EvaporatorTests
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R407C);
    private static readonly Temperature Temperature = 278.15.Kelvins();
    private static readonly TemperatureDelta Superheat = TemperatureDelta.FromDegreesCelsius(8);

    public EvaporatorTests() =>
        Evaporator = new Evaporator(Refrigerant.Name, Temperature, Superheat);

    private Evaporator Evaporator { get; }

    [Theory]
    [InlineData(-74.0)]
    [InlineData(87.0)]
    public void Evaporator_WrongTemperature_ThrowsValidationException(double temperature)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () => _ = new Evaporator(
            Refrigerant.Name, temperature.DegreesCelsius(), Superheat);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Evaporating temperature should be in (-73.15;86.2) °C!*");
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(51.0)]
    public void Evaporator_WrongSuperheat_ThrowsValidationException(double superheat)
    {
        Action action = () => _ = new Evaporator(
            Refrigerant.Name, Temperature, TemperatureDelta.FromKelvins(superheat));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Superheat in the evaporator should be in [0;50] K!*");
    }

    [Fact]
    public void RefrigerantName_Always_ReturnsEnteredName() =>
        Evaporator.RefrigerantName.Should().Be(Refrigerant.Name);

    [Fact]
    public void Temperature_Always_ReturnsEnteredValueInCelsius()
    {
        Evaporator.Temperature.Should().Be(Temperature);
        Evaporator.Temperature.Unit.Should().Be(TemperatureUnit.DegreeCelsius);
    }

    [Fact]
    public void Superheat_Always_ReturnsEnteredValueInKelvins()
    {
        Evaporator.Superheat.Should().Be(Superheat);
        Evaporator.Superheat.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }

    [Fact]
    public void Pressure_Always_ReturnsOutletPressureInKilopascals()
    {
        Evaporator.Pressure.Should().Be(Evaporator.Outlet.Pressure);
        Evaporator.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Outlet_Always_ReturnsSuperheatedRefrigerant() =>
        Evaporator.Outlet.Should().Be(Refrigerant.Superheated(Temperature, Superheat));
}