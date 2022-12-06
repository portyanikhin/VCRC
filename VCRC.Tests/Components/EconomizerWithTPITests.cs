using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests;

public class EconomizerWithTPITests
{
    private static readonly TemperatureDelta TemperatureDifference = 5.DegreesCelsius();

    public EconomizerWithTPITests() =>
        Economizer = new EconomizerWithTPI(TemperatureDifference);

    private EconomizerWithTPI Economizer { get; }

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
        Economizer.TemperatureDifference.Should().Be(TemperatureDifference);
        Economizer.TemperatureDifference.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }
}