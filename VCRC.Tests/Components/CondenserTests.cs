using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class CondenserTests : IClassFixture<ComparisonFixture>
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R407C);
    private static readonly Temperature Temperature = 323.15.Kelvins();
    private static readonly TemperatureDelta Subcooling = TemperatureDelta.FromDegreesCelsius(3);
    private readonly ComparisonFixture _comparison;
    private readonly Condenser _condenser;

    public CondenserTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _condenser = new Condenser(Refrigerant.Name, Temperature, Subcooling);
    }

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
        _condenser.RefrigerantName.Should().Be(Refrigerant.Name);

    [Fact]
    public void Temperature_Always_ReturnsEnteredValueInCelsius()
    {
        _condenser.Temperature.Equals(Temperature, _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
        _condenser.Temperature.Unit.Should().Be(TemperatureUnit.DegreeCelsius);
    }

    [Fact]
    public void Subcooling_Always_ReturnsEnteredValueInKelvins()
    {
        _condenser.Subcooling.Equals(Subcooling, _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
        _condenser.Subcooling.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }

    [Fact]
    public void Pressure_Always_ReturnsOutletPressureInKilopascals()
    {
        _condenser.Pressure.Equals(_condenser.Outlet.Pressure, _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
        _condenser.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Outlet_Always_ReturnsSubcooledRefrigerant() =>
        _condenser.Outlet.Should().Be(Refrigerant.Subcooled(Temperature, Subcooling));
}