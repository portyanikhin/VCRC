using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests;

public class RecuperatorTests
{
    private static readonly TemperatureDelta TemperatureDifference =
        TemperatureDelta.FromDegreesCelsius(5);

    public RecuperatorTests() =>
        Recuperator = new Recuperator(TemperatureDifference);

    private Recuperator Recuperator { get; }

    [Theory]
    [InlineData(0.0)]
    [InlineData(50.0)]
    public static void Recuperator_WrongSuperheat_ThrowsValidationException(double superheat)
    {
        Action action = () => _ = new Recuperator(superheat.Kelvins());
        action.Should().Throw<ValidationException>().WithMessage(
            "*Temperature difference at recuperator 'hot' side should be in (0;50) K!*");
    }

    [Fact]
    public void TemperatureDifference_Always_ReturnsEnteredValueInKelvins()
    {
        Recuperator.TemperatureDifference.Should().Be(TemperatureDifference);
        Recuperator.TemperatureDifference.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }
}