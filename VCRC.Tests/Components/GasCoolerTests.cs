using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class GasCoolerTests : IClassFixture<ComparisonFixture>
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R744);
    private static readonly Temperature Temperature = 308.15.Kelvins();
    private static readonly Pressure Pressure = 8.Megapascals();
    private readonly ComparisonFixture _comparison;
    private readonly GasCooler _gasCooler;
    private readonly GasCooler _gasCoolerWithSpecifiedPressure;

    public GasCoolerTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _gasCooler = new GasCooler(Refrigerant.Name, Temperature);
        _gasCoolerWithSpecifiedPressure =
            new GasCooler(Refrigerant.Name, Temperature, Pressure);
    }

    [Fact]
    public void GasCooler_AnyRefrigerantOtherThanR744WithUnspecifiedPressure_ThrowsArgumentException()
    {
        Action action = () => _ = new GasCooler(FluidsList.R729, Temperature);
        action.Should().Throw<ArgumentException>().WithMessage(
            "It is impossible to automatically calculate the absolute pressure in the gas cooler! " +
            "It is necessary to define it.");
    }

    [Fact]
    public void GasCooler_WrongTemperature_ThrowsValidationException()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () => _ = new GasCooler(Refrigerant.Name, 30.DegreesCelsius());
        action.Should().Throw<ValidationException>()
            .WithMessage("*Gas cooler outlet temperature should be greater than 30.98 °C!*");
    }

    [Fact]
    public void GasCooler_WrongPressure_ThrowsValidationException()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Action action = () => _ = new GasCooler(Refrigerant.Name, Temperature, Pressure.Zero);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Gas cooler absolute pressure should be greater than 7.38 MPa!*");
    }

    [Fact]
    public void RefrigerantName_Always_ReturnsEnteredName() =>
        _gasCooler.RefrigerantName.Should().Be(Refrigerant.Name);

    [Fact]
    public void Temperature_Always_ReturnsEnteredValueInCelsius()
    {
        _gasCooler.Temperature.Equals(Temperature, _comparison.Tolerance.Kelvins())
            .Should().BeTrue();
        _gasCooler.Temperature.Unit.Should().Be(TemperatureUnit.DegreeCelsius);
    }

    [Fact]
    public void Pressure_IfUnspecified_ReturnsOptimalValueByYangEtAlInKilopascals()
    {
        _gasCooler.Pressure.Equals((2.759 * _gasCooler.Temperature.DegreesCelsius - 9.912).Bars(),
                _comparison.Tolerance.Pascals())
            .Should().BeTrue();
        _gasCooler.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Pressure_IfSpecified_ReturnsSpecifiedValueInKilopascals()
    {
        _gasCoolerWithSpecifiedPressure.Pressure.Equals(Pressure, _comparison.Tolerance.Pascals())
            .Should().BeTrue();
        _gasCoolerWithSpecifiedPressure.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Outlet_Always_ReturnsRefrigerantAtPressureAndTemperature() =>
        _gasCooler.Outlet.Should().Be(
            Refrigerant.WithState(Input.Pressure(_gasCooler.Pressure),
                Input.Temperature(Temperature)));
}
