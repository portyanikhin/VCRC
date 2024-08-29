namespace VCRC.Tests;

public sealed class CompressorTests : IClassFixture<ComparisonFixture>
{
    private readonly ComparisonFixture _comparison;
    private readonly Ratio _efficiency;
    private readonly ICompressor _sut;

    public CompressorTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _efficiency = 0.8.DecimalFractions();
        _sut = new Compressor(_efficiency);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(100.0)]
    public static void Compressor_WrongEfficiency_ThrowsValidationException(double efficiency)
    {
        Action action = () => _ = new Compressor(efficiency.Percent());
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Isentropic efficiency of the compressor should be in (0;100) %!*");
    }

    [Fact]
    public void Efficiency_Always_ReturnsEnteredValueInPercents()
    {
        _sut.Efficiency.Equals(_efficiency, _comparison.Tolerance.Percent()).Should().BeTrue();
        _sut.Efficiency.Unit.Should().Be(RatioUnit.Percent);
    }
}
