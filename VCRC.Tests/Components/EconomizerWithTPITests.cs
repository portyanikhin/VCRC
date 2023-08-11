using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests;

public class EconomizerWithTPITests : IClassFixture<ComparisonFixture>
{
    private readonly ComparisonFixture _comparison;
    private readonly IAuxiliaryHeatExchanger _sut;
    private readonly TemperatureDelta _temperatureDifference;

    public EconomizerWithTPITests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _temperatureDifference = 5.DegreesCelsius();
        _sut = new EconomizerWithTPI(_temperatureDifference);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(50.0)]
    public static void Economizer_WrongTemperatureDifference_ThrowsValidationException(
        double temperatureDifference
    )
    {
        Action action = () =>
            _ = new EconomizerWithTPI(temperatureDifference.Kelvins());
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*Temperature difference at the economizer 'cold' side "
                    + "should be in (0;50) K!*"
            );
    }

    [Fact]
    public void TemperatureDifference_Always_ReturnsEnteredValueInKelvins()
    {
        _sut.TemperatureDifference
            .Equals(_temperatureDifference, _comparison.Tolerance.Kelvins())
            .Should()
            .BeTrue();
        _sut.TemperatureDifference.Unit
            .Should()
            .Be(TemperatureDeltaUnit.Kelvin);
    }
}
