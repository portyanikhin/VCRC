using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class GasCoolerTests
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R744);
    private static readonly Temperature Temperature = 308.15.Kelvins();
    private static readonly Pressure Pressure = 8.Megapascals();

    public GasCoolerTests()
    {
        GasCooler = new GasCooler(Refrigerant.Name, Temperature);
        GasCoolerWithSpecifiedPressure =
            new GasCooler(Refrigerant.Name, Temperature, Pressure);
    }

    private GasCooler GasCooler { get; }
    private GasCooler GasCoolerWithSpecifiedPressure { get; }

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
        GasCooler.RefrigerantName.Should().Be(Refrigerant.Name);

    [Fact]
    public void Temperature_Always_ReturnsEnteredValueInCelsius()
    {
        GasCooler.Temperature.Should().Be(Temperature);
        GasCooler.Temperature.Unit.Should().Be(TemperatureUnit.DegreeCelsius);
    }

    [Fact]
    public void Pressure_IfUnspecified_ReturnsOptimalValueByYangEtAlInKilopascals()
    {
        GasCooler.Pressure.Should().Be(
            (2.759 * GasCooler.Temperature.DegreesCelsius - 9.912).Bars());
        GasCooler.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Pressure_IfSpecified_ReturnsSpecifiedValueInKilopascals()
    {
        GasCoolerWithSpecifiedPressure.Pressure.Should().Be(Pressure);
        GasCoolerWithSpecifiedPressure.Pressure.Unit.Should().Be(PressureUnit.Kilopascal);
    }

    [Fact]
    public void Outlet_Always_ReturnsRefrigerantAtPressureAndTemperature() =>
        GasCooler.Outlet.Should().Be(
            Refrigerant.WithState(Input.Pressure(GasCooler.Pressure),
                Input.Temperature(Temperature)));
}