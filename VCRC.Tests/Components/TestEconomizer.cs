using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Tests.Components;

public static class TestEconomizer
{
    [TestCase(-1)]
    [TestCase(51)]
    public static void TestWrongSuperheat(double superheat)
    {
        Action action = () =>
            _ = new Economizer(5.Kelvins(), superheat.Kelvins());
        action.Should().Throw<ValidationException>()
            .WithMessage("*Superheat in the economizer should be in [0;50] K!*");
    }
}