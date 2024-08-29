using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public sealed class RefrigerantTests(ComparisonFixture comparison)
    : IClassFixture<ComparisonFixture>
{
    private readonly SpecificEnergy _enthalpyDelta = 50.KilojoulesPerKilogram();
    private readonly Ratio _isentropicEfficiency = 80.Percent();

    private readonly IRefrigerant _sut = new Refrigerant(FluidsList.R718).WithState(
        Input.Pressure(1.Atmospheres()),
        Input.Temperature(150.DegreesCelsius())
    );

    private readonly TemperatureDelta _temperatureDelta = TemperatureDelta.FromKelvins(10);

    private Pressure HighPressure => 2 * _sut.Pressure;
    private Pressure LowPressure => 0.5 * _sut.Pressure;

    [Fact]
    public static void Refrigerant_WrongName_ThrowsValidationException()
    {
        Action action = () => _ = new Refrigerant(FluidsList.Water);
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*The selected fluid is not a refrigerant (its name should start with 'R')!*"
            );
    }

    [Fact]
    public void CriticalPressure_Water_Returns22e6() =>
        _sut.CriticalPressure.Pascals.Should().Be(22.064e6);

    [Fact]
    public void CriticalTemperature_Water_Returns373() =>
        _sut.CriticalTemperature.DegreesCelsius.Should().Be(373.946);

    [Fact]
    public void TriplePressure_Water_Returns611() =>
        _sut.TriplePressure.Pascals.Should().Be(611.65480089686844);

    [Fact]
    public void TripleTemperature_Water_ReturnsZero() =>
        _sut.TripleTemperature.DegreesCelsius.Should().Be(0.010000000000047748);

    [Theory]
    [InlineData(FluidsList.R32)]
    [InlineData(FluidsList.R134a)]
    public static void Glide_PureRefrigerants_ReturnsZero(FluidsList name)
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.Glide.Kelvins.Should().Be(0);
    }

    [Theory]
    [InlineData(FluidsList.R404A, 0.75017192257570287)]
    [InlineData(FluidsList.R407C, 6.9953896623448202)]
    public void Glide_ZeotropicBlends_ReturnsNotZero(FluidsList name, double expected)
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.Glide.Kelvins.Should().BeApproximately(expected, comparison.Tolerance);
    }

    [Theory]
    [InlineData(FluidsList.R507A, 0.0014729570078202414)]
    [InlineData(FluidsList.R511A, 0.0088663065580476541)]
    public void Glide_AzeotropicBlends_ReturnsApproximatelyZero(FluidsList name, double expected)
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.Glide.Kelvins.Should().BeApproximately(expected, comparison.Tolerance);
    }

    [Theory]
    [InlineData(FluidsList.R32)]
    [InlineData(FluidsList.R134a)]
    [InlineData(FluidsList.R507A)]
    [InlineData(FluidsList.R511A)]
    public static void HasGlide_PureRefrigerantsAndAzeotropicBlends_ReturnsFalse(FluidsList name)
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.HasGlide.Should().BeFalse();
    }

    [Theory]
    [InlineData(FluidsList.R404A)]
    [InlineData(FluidsList.R407C)]
    public static void HasGlide_ZeotropicBlends_ReturnsTrue(FluidsList name)
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.HasGlide.Should().BeTrue();
    }

    [Theory]
    [InlineData(FluidsList.R32)]
    [InlineData(FluidsList.R134a)]
    public static void IsSingleComponent_PureRefrigerants_ReturnsTrue(FluidsList name)
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.IsSingleComponent.Should().BeTrue();
    }

    [Theory]
    [InlineData(FluidsList.R404A)]
    [InlineData(FluidsList.R407C)]
    [InlineData(FluidsList.R507A)]
    [InlineData(FluidsList.R511A)]
    public static void IsSingleComponent_ZeotropicAndAzeotropicBlends_ReturnsFalse(FluidsList name)
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.IsSingleComponent.Should().BeFalse();
    }

    [Theory]
    [InlineData(FluidsList.R507A)]
    [InlineData(FluidsList.R511A)]
    public static void IsAzeotropicBlend_AzeotropicBlends_ReturnsTrue(FluidsList name)
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.IsAzeotropicBlend.Should().BeTrue();
    }

    [Theory]
    [InlineData(FluidsList.R32)]
    [InlineData(FluidsList.R134a)]
    [InlineData(FluidsList.R404A)]
    [InlineData(FluidsList.R407C)]
    public static void IsAzeotropicBlend_AnyRefrigerantsOtherThanAzeotropicBlends_ReturnsFalse(
        FluidsList name
    )
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.IsAzeotropicBlend.Should().BeFalse();
    }

    [Theory]
    [InlineData(FluidsList.R404A)]
    [InlineData(FluidsList.R407C)]
    public static void IsZeotropicBlend_ZeotropicBlends_ReturnsTrue(FluidsList name)
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.IsZeotropicBlend.Should().BeTrue();
    }

    [Theory]
    [InlineData(FluidsList.R32)]
    [InlineData(FluidsList.R134a)]
    [InlineData(FluidsList.R507A)]
    [InlineData(FluidsList.R511A)]
    public static void IsZeotropicBlend_AnyRefrigerantsOtherThanZeotropicBlends_ReturnsFalse(
        FluidsList name
    )
    {
        IRefrigerant refrigerant = new Refrigerant(name);
        refrigerant.IsZeotropicBlend.Should().BeFalse();
    }

    [Fact]
    public void Subcooled_TemperatureAndWrongSubcooling_ThrowsArgumentException()
    {
        Action action = () =>
            _ = _sut.Subcooled(_sut.Temperature, TemperatureDelta.FromKelvins(-5));
        action.Should().Throw<ArgumentException>().WithMessage("Invalid subcooling!");
    }

    [Fact]
    public void Subcooled_TemperatureAndNonZeroSubcooling_ReturnsRefrigerantAtSatPressureAndSubcooledTemperature()
    {
        var bubblePoint = _sut.BubblePointAt(_sut.Temperature);
        _sut.Subcooled(_sut.Temperature, TemperatureDelta.FromKelvins(3))
            .Should()
            .Be(bubblePoint.CoolingTo(bubblePoint.Temperature - TemperatureDelta.FromKelvins(3)));
    }

    [Fact]
    public void Subcooled_TemperatureAndZeroSubcooling_ReturnsBubblePointAtTemperature()
    {
        var bubblePoint = _sut.BubblePointAt(_sut.Temperature);
        _sut.Subcooled(_sut.Temperature, TemperatureDelta.Zero).Should().Be(bubblePoint);
    }

    [Fact]
    public void Subcooled_PressureAndWrongSubcooling_ThrowsArgumentException()
    {
        Action action = () => _ = _sut.Subcooled(_sut.Pressure, TemperatureDelta.FromKelvins(-5));
        action.Should().Throw<ArgumentException>().WithMessage("Invalid subcooling!");
    }

    [Fact]
    public void Subcooled_PressureAndNonZeroSubcooling_ReturnsRefrigerantAtSatPressureAndSubcooledTemperature()
    {
        var bubblePoint = _sut.BubblePointAt(_sut.Pressure);
        _sut.Subcooled(_sut.Pressure, TemperatureDelta.FromKelvins(3))
            .Should()
            .Be(bubblePoint.CoolingTo(bubblePoint.Temperature - TemperatureDelta.FromKelvins(3)));
    }

    [Fact]
    public void Subcooled_PressureAndZeroSubcooling_ReturnsBubblePointAtPressure()
    {
        var bubblePoint = _sut.BubblePointAt(_sut.Pressure);
        _sut.Subcooled(_sut.Pressure, TemperatureDelta.Zero).Should().Be(bubblePoint);
    }

    [Fact]
    public void Superheated_TemperatureAndWrongSuperheat_ThrowsArgumentException()
    {
        Action action = () =>
            _ = _sut.Superheated(_sut.Temperature, TemperatureDelta.FromKelvins(-5));
        action.Should().Throw<ArgumentException>().WithMessage("Invalid superheat!");
    }

    [Fact]
    public void Superheated_TemperatureAndNonZeroSuperheat_ReturnsRefrigerantAtSatPressureAndSuperheatedTemperature()
    {
        var dewPoint = _sut.DewPointAt(_sut.Temperature);
        _sut.Superheated(_sut.Temperature, TemperatureDelta.FromKelvins(8))
            .Should()
            .Be(dewPoint.HeatingTo(dewPoint.Temperature + TemperatureDelta.FromKelvins(8)));
    }

    [Fact]
    public void Superheated_TemperatureAndZeroSuperheat_ReturnsDewPointAtTemperature()
    {
        var dewPoint = _sut.DewPointAt(_sut.Temperature);
        _sut.Superheated(_sut.Temperature, TemperatureDelta.Zero).Should().Be(dewPoint);
    }

    [Fact]
    public void Superheated_PressureAndWrongSuperheat_ThrowsArgumentException()
    {
        Action action = () => _ = _sut.Superheated(_sut.Pressure, TemperatureDelta.FromKelvins(-5));
        action.Should().Throw<ArgumentException>().WithMessage("Invalid superheat!");
    }

    [Fact]
    public void Superheated_PressureAndNonZeroSuperheat_ReturnsRefrigerantAtSatPressureAndSuperheatedTemperature()
    {
        var dewPoint = _sut.DewPointAt(_sut.Pressure);
        _sut.Superheated(_sut.Pressure, TemperatureDelta.FromKelvins(8))
            .Should()
            .Be(dewPoint.HeatingTo(dewPoint.Temperature + TemperatureDelta.FromKelvins(8)));
    }

    [Fact]
    public void Superheated_PressureAndZeroSuperheat_ReturnsDewPointAtAtPressure()
    {
        var dewPoint = _sut.DewPointAt(_sut.Pressure);
        _sut.Superheated(_sut.Pressure, TemperatureDelta.Zero).Should().Be(dewPoint);
    }

    [Fact]
    public void Methods_New_ReturnsInstancesOfTheRefrigerantType()
    {
        _sut.Clone().Should().BeOfType<Refrigerant>();
        _sut.Factory().Should().BeOfType<Refrigerant>();
        _sut.IsentropicCompressionTo(HighPressure).Should().BeOfType<Refrigerant>();
        _sut.CompressionTo(HighPressure, _isentropicEfficiency).Should().BeOfType<Refrigerant>();
        _sut.IsenthalpicExpansionTo(LowPressure).Should().BeOfType<Refrigerant>();
        _sut.IsentropicExpansionTo(LowPressure).Should().BeOfType<Refrigerant>();
        _sut.ExpansionTo(LowPressure, _isentropicEfficiency).Should().BeOfType<Refrigerant>();
        _sut.CoolingTo(_sut.Temperature - _temperatureDelta).Should().BeOfType<Refrigerant>();
        _sut.CoolingTo(_sut.Enthalpy - _enthalpyDelta).Should().BeOfType<Refrigerant>();
        _sut.HeatingTo(_sut.Temperature + _temperatureDelta).Should().BeOfType<Refrigerant>();
        _sut.HeatingTo(_sut.Enthalpy + _enthalpyDelta).Should().BeOfType<Refrigerant>();
        _sut.BubblePointAt(1.Atmospheres()).Should().BeOfType<Refrigerant>();
        _sut.BubblePointAt(100.DegreesCelsius()).Should().BeOfType<Refrigerant>();
        _sut.DewPointAt(1.Atmospheres()).Should().BeOfType<Refrigerant>();
        _sut.DewPointAt(100.DegreesCelsius()).Should().BeOfType<Refrigerant>();
        _sut.TwoPhasePointAt(1.Atmospheres(), 50.Percent()).Should().BeOfType<Refrigerant>();
        _sut.Mixing(
                100.Percent(),
                _sut.CoolingTo(_sut.Temperature - _temperatureDelta),
                200.Percent(),
                _sut.HeatingTo(_sut.Temperature + _temperatureDelta)
            )
            .Should()
            .BeOfType<Refrigerant>();
    }
}
