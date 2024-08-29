using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public sealed class GasCoolerTests : IClassFixture<ComparisonFixture>
{
    private readonly ComparisonFixture _comparison;
    private readonly Pressure _pressure;
    private readonly IRefrigerant _refrigerant;
    private readonly IHeatReleaser _sut;
    private readonly IHeatReleaser _sutWithSpecifiedPressure;
    private readonly Temperature _temperature;

    public GasCoolerTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _refrigerant = new Refrigerant(FluidsList.R744);
        _temperature = 308.15.Kelvins();
        _pressure = 8.Megapascals();
        _sut = new GasCooler(_refrigerant.Name, _temperature);
        _sutWithSpecifiedPressure = new GasCooler(_refrigerant.Name, _temperature, _pressure);
    }

    [Fact]
    public void GasCooler_AnyRefrigerantOtherThanR744WithUnspecifiedPressure_ThrowsArgumentException()
    {
        Action action = () => _ = new GasCooler(FluidsList.R729, _temperature);
        action
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(
                "It is impossible to automatically calculate "
                    + "the absolute pressure in the gas cooler! It is necessary to define it."
            );
    }

    [Fact]
    public void GasCooler_WrongTemperature_ThrowsValidationException()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () => _ = new GasCooler(_refrigerant.Name, 30.DegreesCelsius());
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Gas cooler outlet temperature should be greater than 30.98 °C!*");
    }

    [Fact]
    public void GasCooler_WrongPressure_ThrowsValidationException()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () => _ = new GasCooler(_refrigerant.Name, _temperature, Pressure.Zero);
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Gas cooler absolute pressure should be greater than 7.38 MPa!*");
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
    public void Pressure_IfUnspecified_ReturnsOptimalValueByYangEtAlInKilopascals()
    {
        _sut.Pressure.Equals(
                (2.759 * _sut.Temperature.DegreesCelsius - 9.912).Bars(),
                _comparison.Tolerance.Pascals()
            )
            .Should()
            .BeTrue();
        _sut.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Pressure_IfSpecified_ReturnsSpecifiedValueInKilopascals()
    {
        _sutWithSpecifiedPressure
            .Pressure.Equals(_pressure, _comparison.Tolerance.Pascals())
            .Should()
            .BeTrue();
        _sutWithSpecifiedPressure.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Outlet_Always_ReturnsRefrigerantAtPressureAndTemperature() =>
        _sut
            .Outlet.Should()
            .Be(
                _refrigerant.WithState(
                    Input.Pressure(_sut.Pressure),
                    Input.Temperature(_temperature)
                )
            );
}
