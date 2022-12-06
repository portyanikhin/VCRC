using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests;

public class EconomizerTests
{
    private static readonly TemperatureDelta TemperatureDifference = 5.DegreesCelsius();
    private static readonly TemperatureDelta Superheat = 5.DegreesCelsius();

    public EconomizerTests() =>
        Economizer = new Economizer(TemperatureDifference, Superheat);

    private Economizer Economizer { get; }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(51.0)]
    public void Economizer_WrongSuperheat_ThrowsValidationException(double superheat)
    {
        Action action = () => _ = new Economizer(TemperatureDifference, superheat.Kelvins());
        action.Should().Throw<ValidationException>()
            .WithMessage("*Superheat in the economizer should be in [0;50] K!*");
    }

    [Fact]
    public void Superheat_Always_ReturnsEnteredValueInKelvins()
    {
        Economizer.Superheat.Should().Be(Superheat);
        Economizer.Superheat.Unit.Should().Be(TemperatureDeltaUnit.Kelvin);
    }
}