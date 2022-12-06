namespace VCRC.Tests;

public class CompressorTests
{
    private static readonly Ratio Efficiency = 0.8.DecimalFractions();

    public CompressorTests() =>
        Compressor = new Compressor(Efficiency);

    private Compressor Compressor { get; }

    [Theory]
    [InlineData(0.0)]
    [InlineData(100.0)]
    public static void Compressor_WrongEfficiency_ThrowsValidationException(double efficiency)
    {
        Action action = () => _ = new Compressor(efficiency.Percent());
        action.Should().Throw<ValidationException>().WithMessage(
            "*Isentropic efficiency of the compressor should be in (0;100) %!*");
    }

    [Fact]
    public void Efficiency_Always_ReturnsEnteredValueInPercents()
    {
        Compressor.Efficiency.Should().Be(Efficiency);
        Compressor.Efficiency.Unit.Should().Be(RatioUnit.Percent);
    }
}