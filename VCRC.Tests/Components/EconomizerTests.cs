using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests;

public class EconomizerTests : IClassFixture<ComparisonFixture>
{
    private static readonly TemperatureDelta TemperatureDifference =
        5.DegreesCelsius();
    private static readonly TemperatureDelta Superheat = 5.DegreesCelsius();
    private readonly ComparisonFixture _comparison;
    private readonly Economizer _economizer;

    public EconomizerTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _economizer = new Economizer(TemperatureDifference, Superheat);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(51.0)]
    public void Economizer_WrongSuperheat_ThrowsValidationException(
        double superheat
    )
    {
        Action action = () =>
            _ = new Economizer(TemperatureDifference, superheat.Kelvins());
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*Superheat in the economizer should be in [0;50] K!*"
            );
    }

    [Fact]
    public void Superheat_Always_ReturnsEnteredValueInKelvins()
    {
        _economizer.Superheat
            .Equals(Superheat, _comparison.Tolerance.Kelvins())
            .Should()
            .BeTrue();
        _economizer.Superheat.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }
}
