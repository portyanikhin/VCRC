using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests;

public sealed class EconomizerTests : IClassFixture<ComparisonFixture>
{
    private readonly ComparisonFixture _comparison;
    private readonly TemperatureDelta _superheat;
    private readonly IEconomizer _sut;
    private readonly TemperatureDelta _temperatureDifference;

    public EconomizerTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _superheat = 5.DegreesCelsius();
        _temperatureDifference = 5.DegreesCelsius();
        _sut = new Economizer(_temperatureDifference, _superheat);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(51.0)]
    public void Economizer_WrongSuperheat_ThrowsValidationException(double superheat)
    {
        Action action = () => _ = new Economizer(_temperatureDifference, superheat.Kelvins());
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Superheat in the economizer should be in [0;50] K!*");
    }

    [Fact]
    public void Superheat_Always_ReturnsEnteredValueInKelvins()
    {
        _sut.Superheat.Equals(_superheat, _comparison.Tolerance.Kelvins()).Should().BeTrue();
        _sut.Superheat.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }
}
