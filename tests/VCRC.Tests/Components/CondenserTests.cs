using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public sealed class CondenserTests : IClassFixture<ComparisonFixture>
{
    private readonly ComparisonFixture _comparison;
    private readonly IRefrigerant _refrigerant;
    private readonly TemperatureDelta _subcooling;
    private readonly ICondenser _sut;
    private readonly Temperature _temperature;

    public CondenserTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _refrigerant = new Refrigerant(FluidsList.R407C);
        _temperature = 323.15.Kelvins();
        _subcooling = TemperatureDelta.FromDegreesCelsius(3);
        _sut = new Condenser(_refrigerant.Name, _temperature, _subcooling);
    }

    [Theory]
    [InlineData(-74.0)]
    [InlineData(87.0)]
    public void Condenser_WrongTemperature_ThrowsValidationException(double temperature)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () =>
            _ = new Condenser(_refrigerant.Name, temperature.DegreesCelsius(), _subcooling);
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Condensing temperature should be in (-73.15;86.2) °C!*");
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(51.0)]
    public void Condenser_WrongSubcooling_ThrowsValidationException(double subcooling)
    {
        Action action = () =>
            _ = new Condenser(
                _refrigerant.Name,
                _temperature,
                TemperatureDelta.FromKelvins(subcooling)
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Subcooling in the condenser should be in [0;50] K!*");
    }

    [Fact]
    public void RefrigerantName_Always_ReturnsEnteredName() =>
        _sut.RefrigerantName.Should().Be(_refrigerant.Name);

    [Fact]
    public void Temperature_Always_ReturnsEnteredValueInCelsius()
    {
        _sut.Temperature.Equals(_temperature, _comparison.Tolerance.DegreesCelsius())
            .Should()
            .BeTrue();
        _sut.Temperature.Unit.Should().Be(TemperatureUnit.DegreeCelsius);
    }

    [Fact]
    public void Subcooling_Always_ReturnsEnteredValueInKelvins()
    {
        _sut.Subcooling.Equals(_subcooling, TemperatureDelta.FromKelvins(_comparison.Tolerance))
            .Should()
            .BeTrue();
        _sut.Subcooling.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }

    [Fact]
    public void Pressure_Always_ReturnsOutletPressureInKilopascals()
    {
        _sut.Pressure.Equals(_sut.Outlet.Pressure, _comparison.Tolerance.Pascals())
            .Should()
            .BeTrue();
        _sut.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Outlet_Always_ReturnsSubcooledRefrigerant() =>
        _sut.Outlet.Should().Be(_refrigerant.Subcooled(_temperature, _subcooling));
}
