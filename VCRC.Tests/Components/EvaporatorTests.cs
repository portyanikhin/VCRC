using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class EvaporatorTests : IClassFixture<ComparisonFixture>
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R407C);
    private static readonly Temperature Temperature = 278.15.Kelvins();
    private static readonly TemperatureDelta Superheat = TemperatureDelta.FromDegreesCelsius(8);
    private readonly ComparisonFixture _comparison;
    private readonly Evaporator _evaporator;

    public EvaporatorTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _evaporator = new Evaporator(Refrigerant.Name, Temperature, Superheat);
    }

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
        _evaporator.RefrigerantName.Should().Be(Refrigerant.Name);

    [Fact]
    public void Temperature_Always_ReturnsEnteredValueInCelsius()
    {
        _evaporator.Temperature.Equals(Temperature, _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
        _evaporator.Temperature.Unit.Should().Be(TemperatureUnit.DegreeCelsius);
    }

    [Fact]
    public void Superheat_Always_ReturnsEnteredValueInKelvins()
    {
        _evaporator.Superheat.Equals(Superheat, _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
        _evaporator.Superheat.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }

    [Fact]
    public void Pressure_Always_ReturnsOutletPressureInKilopascals()
    {
        _evaporator.Pressure.Should().Be(_evaporator.Outlet.Pressure);
        _evaporator.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Outlet_Always_ReturnsSuperheatedRefrigerant() =>
        _evaporator.Outlet.Should().Be(Refrigerant.Superheated(Temperature, Superheat));
}