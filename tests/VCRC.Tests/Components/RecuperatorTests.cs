using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests;

public class RecuperatorTests : IClassFixture<ComparisonFixture>
{
    private readonly ComparisonFixture _comparison;
    private readonly IAuxiliaryHeatExchanger _recuperator;
    private readonly TemperatureDelta _temperatureDifference;

    public RecuperatorTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _temperatureDifference = TemperatureDelta.FromDegreesCelsius(5);
        _recuperator = new Recuperator(_temperatureDifference);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(50.0)]
    public static void Recuperator_WrongSuperheat_ThrowsValidationException(
        double superheat
    )
    {
        Action action = () => _ = new Recuperator(superheat.Kelvins());
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*Temperature difference at the recuperator 'hot' side "
                    + "should be in (0;50) K!*"
            );
    }

    [Fact]
    public void TemperatureDifference_Always_ReturnsEnteredValueInKelvins()
    {
        _recuperator
            .TemperatureDifference
            .Equals(_temperatureDifference, _comparison.Tolerance.Kelvins())
            .Should()
            .BeTrue();
        _recuperator
            .TemperatureDifference
            .Unit
            .Should()
            .Be(TemperatureDeltaUnit.Kelvin);
    }
}
