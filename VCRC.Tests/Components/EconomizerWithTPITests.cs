using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests;

public class EconomizerWithTPITests : IClassFixture<ComparisonFixture>
{
    private static readonly TemperatureDelta TemperatureDifference = 5.DegreesCelsius();
    private readonly ComparisonFixture _comparison;
    private readonly EconomizerWithTPI _economizer;

    public EconomizerWithTPITests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _economizer = new EconomizerWithTPI(TemperatureDifference);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(50.0)]
    public static void Economizer_WrongTemperatureDifference_ThrowsValidationException(
        double temperatureDifference)
    {
        Action action = () => _ = new EconomizerWithTPI(temperatureDifference.Kelvins());
        action.Should().Throw<ValidationException>().WithMessage(
            "*Temperature difference at the economizer 'cold' side should be in (0;50) K!*");
    }

    [Fact]
    public void TemperatureDifference_Always_ReturnsEnteredValueInKelvins()
    {
        _economizer.TemperatureDifference.Equals(TemperatureDifference, _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
        _economizer.TemperatureDifference.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }
}