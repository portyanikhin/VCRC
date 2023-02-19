namespace VCRC.Tests;

public class CompressorTests : IClassFixture<ComparisonFixture>
{
    private static readonly Ratio Efficiency = 0.8.DecimalFractions();
    private readonly ComparisonFixture _comparison;
    private readonly Compressor _compressor;

    public CompressorTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _compressor = new Compressor(Efficiency);
    }

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
        _compressor.Efficiency.Equals(Efficiency, _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
        _compressor.Efficiency.Unit.Should().Be(RatioUnit.Percent);
    }
}