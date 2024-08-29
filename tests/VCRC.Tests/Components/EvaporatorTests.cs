using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public sealed class EvaporatorTests : IClassFixture<ComparisonFixture>
{
    private readonly ComparisonFixture _comparison;
    private readonly IRefrigerant _refrigerant;
    private readonly TemperatureDelta _superheat;
    private readonly IEvaporator _sut;
    private readonly Temperature _temperature;

    public EvaporatorTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _refrigerant = new Refrigerant(FluidsList.R407C);
        _temperature = 278.15.Kelvins();
        _superheat = TemperatureDelta.FromDegreesCelsius(8);
        _sut = new Evaporator(_refrigerant.Name, _temperature, _superheat);
    }

    [Theory]
    [InlineData(-74.0)]
    [InlineData(87.0)]
    public void Evaporator_WrongTemperature_ThrowsValidationException(double temperature)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () =>
            _ = new Evaporator(_refrigerant.Name, temperature.DegreesCelsius(), _superheat);
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Evaporating temperature should be in (-73.15;86.2) °C!*");
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(51.0)]
    public void Evaporator_WrongSuperheat_ThrowsValidationException(double superheat)
    {
        Action action = () =>
            _ = new Evaporator(
                _refrigerant.Name,
                _temperature,
                TemperatureDelta.FromKelvins(superheat)
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Superheat in the evaporator should be in [0;50] K!*");
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
    public void Superheat_Always_ReturnsEnteredValueInKelvins()
    {
        _sut.Superheat.Equals(_superheat, TemperatureDelta.FromKelvins(_comparison.Tolerance))
            .Should()
            .BeTrue();
        _sut.Superheat.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }

    [Fact]
    public void Pressure_Always_ReturnsOutletPressureInKilopascals()
    {
        _sut.Pressure.Should().Be(_sut.Outlet.Pressure);
        _sut.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Outlet_Always_ReturnsSuperheatedRefrigerant() =>
        _sut.Outlet.Should().Be(_refrigerant.Superheated(_temperature, _superheat));
}
