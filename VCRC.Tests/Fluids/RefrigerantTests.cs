using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class RefrigerantTests
{
    private static readonly Ratio IsentropicEfficiency = 80.Percent();
    private static readonly TemperatureDelta TemperatureDelta = TemperatureDelta.FromKelvins(10);
    private static readonly SpecificEnergy EnthalpyDelta = 50.KilojoulesPerKilogram();

    public RefrigerantTests() =>
        Refrigerant = new Refrigerant(FluidsList.R718)
            .WithState(Input.Pressure(1.Atmospheres()),
                Input.Temperature(150.DegreesCelsius()));

    private Refrigerant Refrigerant { get; }
    private Pressure HighPressure => 2 * Refrigerant.Pressure;
    private Pressure LowPressure => 0.5 * Refrigerant.Pressure;

    [Fact]
    public static void Refrigerant_WrongName_ThrowsValidationException()
    {
        Action action = () => _ = new Refrigerant(FluidsList.Water);
        action.Should().Throw<ValidationException>().WithMessage(
            "*The selected fluid is not a refrigerant (its name should start with 'R')!*");
    }

    [Fact]
    public void CriticalPressure_Water_Returns22e6() =>
        Refrigerant.CriticalPressure.Pascals.Should().Be(22.064e6);

    [Fact]
    public void CriticalTemperature_Water_Returns373() =>
        Refrigerant.CriticalTemperature.DegreesCelsius.Should().Be(373.946);

    [Fact]
    public void TriplePressure_Water_Returns611() =>
        Refrigerant.TriplePressure.Pascals.Should().Be(611.65480089686844);

    [Fact]
    public void TripleTemperature_Water_ReturnsZero() =>
        Refrigerant.TripleTemperature.DegreesCelsius.Should().Be(0.010000000000047748);

    [Theory]
    [InlineData(FluidsList.R32)]
    [InlineData(FluidsList.R134a)]
    public static void Glide_PureRefrigerants_ReturnsZero(FluidsList name) =>
        new Refrigerant(name).Glide.Kelvins.Should().Be(0);

    [Theory]
    [InlineData(FluidsList.R404A, 0.75017192257570287)]
    [InlineData(FluidsList.R407C, 6.9953896623448202)]
    public static void Glide_ZeotropicBlends_ReturnsNotZero(FluidsList name, double expected) =>
        new Refrigerant(name).Glide.Kelvins.Should().Be(expected);

    [Theory]
    [InlineData(FluidsList.R507A, 0.0014729570078202414)]
    [InlineData(FluidsList.R511A, 0.0088663065580476541)]
    public static void Glide_AzeotropicBlends_ReturnsApproximatelyZero(FluidsList name, double expected) =>
        new Refrigerant(name).Glide.Kelvins.Should().Be(expected);

    [Theory]
    [InlineData(FluidsList.R32)]
    [InlineData(FluidsList.R134a)]
    [InlineData(FluidsList.R507A)]
    [InlineData(FluidsList.R511A)]
    public static void HasGlide_PureRefrigerantsAndAzeotropicBlends_ReturnsFalse(FluidsList name) =>
        new Refrigerant(name).HasGlide.Should().BeFalse();

    [Theory]
    [InlineData(FluidsList.R404A)]
    [InlineData(FluidsList.R407C)]
    public static void HasGlide_ZeotropicBlends_ReturnsTrue(FluidsList name) =>
        new Refrigerant(name).HasGlide.Should().BeTrue();

    [Theory]
    [InlineData(FluidsList.R32)]
    [InlineData(FluidsList.R134a)]
    public static void IsSingleComponent_PureRefrigerants_ReturnsTrue(FluidsList name) =>
        new Refrigerant(name).IsSingleComponent.Should().BeTrue();

    [Theory]
    [InlineData(FluidsList.R404A)]
    [InlineData(FluidsList.R407C)]
    [InlineData(FluidsList.R507A)]
    [InlineData(FluidsList.R511A)]
    public static void IsSingleComponent_ZeotropicAndAzeotropicBlends_ReturnsFalse(FluidsList name) =>
        new Refrigerant(name).IsSingleComponent.Should().BeFalse();

    [Theory]
    [InlineData(FluidsList.R507A)]
    [InlineData(FluidsList.R511A)]
    public static void IsAzeotropicBlend_AzeotropicBlends_ReturnsTrue(FluidsList name) =>
        new Refrigerant(name).IsAzeotropicBlend.Should().BeTrue();

    [Theory]
    [InlineData(FluidsList.R32)]
    [InlineData(FluidsList.R134a)]
    [InlineData(FluidsList.R404A)]
    [InlineData(FluidsList.R407C)]
    public static void IsAzeotropicBlend_AnyRefrigerantsOtherThanAzeotropicBlends_ReturnsFalse(FluidsList name) =>
        new Refrigerant(name).IsAzeotropicBlend.Should().BeFalse();

    [Theory]
    [InlineData(FluidsList.R404A)]
    [InlineData(FluidsList.R407C)]
    public static void IsZeotropicBlend_ZeotropicBlends_ReturnsTrue(FluidsList name) =>
        new Refrigerant(name).IsZeotropicBlend.Should().BeTrue();

    [Theory]
    [InlineData(FluidsList.R32)]
    [InlineData(FluidsList.R134a)]
    [InlineData(FluidsList.R507A)]
    [InlineData(FluidsList.R511A)]
    public static void IsZeotropicBlend_AnyRefrigerantsOtherThanZeotropicBlends_ReturnsFalse(FluidsList name) =>
        new Refrigerant(name).IsZeotropicBlend.Should().BeFalse();

    [Fact]
    public void Subcooled_TemperatureAndWrongSubcooling_ThrowsArgumentException()
    {
        Action action = () => _ = Refrigerant.Subcooled(
            Refrigerant.Temperature, TemperatureDelta.FromKelvins(-5));
        action.Should().Throw<ArgumentException>().WithMessage("Invalid subcooling!");
    }

    [Fact]
    public void Subcooled_TemperatureAndNonZeroSubcooling_ReturnsRefrigerantAtSatPressureAndSubcooledTemperature()
    {
        var bubblePoint = Refrigerant.BubblePointAt(Refrigerant.Temperature);
        Refrigerant.Subcooled(Refrigerant.Temperature, TemperatureDelta.FromKelvins(3))
            .Should().Be(bubblePoint.CoolingTo(bubblePoint.Temperature - TemperatureDelta.FromKelvins(3)));
    }

    [Fact]
    public void Subcooled_TemperatureAndZeroSubcooling_ReturnsBubblePointAtTemperature()
    {
        var bubblePoint = Refrigerant.BubblePointAt(Refrigerant.Temperature);
        Refrigerant.Subcooled(Refrigerant.Temperature, TemperatureDelta.Zero)
            .Should().Be(bubblePoint);
    }

    [Fact]
    public void Subcooled_PressureAndWrongSubcooling_ThrowsArgumentException()
    {
        Action action = () => _ = Refrigerant.Subcooled(
            Refrigerant.Pressure, TemperatureDelta.FromKelvins(-5));
        action.Should().Throw<ArgumentException>().WithMessage("Invalid subcooling!");
    }

    [Fact]
    public void Subcooled_PressureAndNonZeroSubcooling_ReturnsRefrigerantAtSatPressureAndSubcooledTemperature()
    {
        var bubblePoint = Refrigerant.BubblePointAt(Refrigerant.Pressure);
        Refrigerant.Subcooled(Refrigerant.Pressure, TemperatureDelta.FromKelvins(3))
            .Should().Be(bubblePoint.CoolingTo(bubblePoint.Temperature - TemperatureDelta.FromKelvins(3)));
    }

    [Fact]
    public void Subcooled_PressureAndZeroSubcooling_ReturnsBubblePointAtPressure()
    {
        var bubblePoint = Refrigerant.BubblePointAt(Refrigerant.Pressure);
        Refrigerant.Subcooled(Refrigerant.Pressure, TemperatureDelta.Zero)
            .Should().Be(bubblePoint);
    }

    [Fact]
    public void Superheated_TemperatureAndWrongSuperheat_ThrowsArgumentException()
    {
        Action action = () => _ = Refrigerant.Superheated(
            Refrigerant.Temperature, TemperatureDelta.FromKelvins(-5));
        action.Should().Throw<ArgumentException>().WithMessage("Invalid superheat!");
    }

    [Fact]
    public void Superheated_TemperatureAndNonZeroSuperheat_ReturnsRefrigerantAtSatPressureAndSuperheatedTemperature()
    {
        var dewPoint = Refrigerant.DewPointAt(Refrigerant.Temperature);
        Refrigerant.Superheated(Refrigerant.Temperature, TemperatureDelta.FromKelvins(8))
            .Should().Be(dewPoint.HeatingTo(dewPoint.Temperature + TemperatureDelta.FromKelvins(8)));
    }

    [Fact]
    public void Superheated_TemperatureAndZeroSuperheat_ReturnsDewPointAtTemperature()
    {
        var dewPoint = Refrigerant.DewPointAt(Refrigerant.Temperature);
        Refrigerant.Superheated(Refrigerant.Temperature, TemperatureDelta.Zero)
            .Should().Be(dewPoint);
    }

    [Fact]
    public void Superheated_PressureAndWrongSuperheat_ThrowsArgumentException()
    {
        Action action = () => _ = Refrigerant.Superheated(
            Refrigerant.Pressure, TemperatureDelta.FromKelvins(-5));
        action.Should().Throw<ArgumentException>().WithMessage("Invalid superheat!");
    }

    [Fact]
    public void Superheated_PressureAndNonZeroSuperheat_ReturnsRefrigerantAtSatPressureAndSuperheatedTemperature()
    {
        var dewPoint = Refrigerant.DewPointAt(Refrigerant.Pressure);
        Refrigerant.Superheated(Refrigerant.Pressure, TemperatureDelta.FromKelvins(8))
            .Should().Be(dewPoint.HeatingTo(dewPoint.Temperature + TemperatureDelta.FromKelvins(8)));
    }

    [Fact]
    public void Superheated_PressureAndZeroSuperheat_ReturnsDewPointAtAtPressure()
    {
        var dewPoint = Refrigerant.DewPointAt(Refrigerant.Pressure);
        Refrigerant.Superheated(Refrigerant.Pressure, TemperatureDelta.Zero)
            .Should().Be(dewPoint);
    }

    [Fact]
    public void Processes_Override_ReturnsInstancesOfTheRefrigerantType()
    {
        Refrigerant.IsentropicCompressionTo(HighPressure)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.CompressionTo(HighPressure, IsentropicEfficiency)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.IsenthalpicExpansionTo(LowPressure)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.IsentropicExpansionTo(LowPressure)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.ExpansionTo(LowPressure, IsentropicEfficiency)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.CoolingTo(Refrigerant.Temperature - TemperatureDelta)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.CoolingTo(Refrigerant.Enthalpy - EnthalpyDelta)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.HeatingTo(Refrigerant.Temperature + TemperatureDelta)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.HeatingTo(Refrigerant.Enthalpy + EnthalpyDelta)
            .Should().BeOfType<Refrigerant>();
        Refrigerant.BubblePointAt(1.Atmospheres())
            .Should().BeOfType<Refrigerant>();
        Refrigerant.BubblePointAt(100.DegreesCelsius())
            .Should().BeOfType<Refrigerant>();
        Refrigerant.DewPointAt(1.Atmospheres())
            .Should().BeOfType<Refrigerant>();
        Refrigerant.DewPointAt(100.DegreesCelsius())
            .Should().BeOfType<Refrigerant>();
        Refrigerant.TwoPhasePointAt(1.Atmospheres(), 50.Percent())
            .Should().BeOfType<Refrigerant>();
        Refrigerant.Mixing(
                100.Percent(),
                Refrigerant.CoolingTo(Refrigerant.Temperature - TemperatureDelta),
                200.Percent(),
                Refrigerant.HeatingTo(Refrigerant.Temperature + TemperatureDelta))
            .Should().BeOfType<Refrigerant>();
    }
}