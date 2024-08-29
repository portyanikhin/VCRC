namespace VCRC.Tests;

public sealed class EjectorTests : IClassFixture<ComparisonFixture>
{
    private readonly ComparisonFixture _comparison;
    private readonly Ratio _diffuserEfficiency;
    private readonly Ratio _nozzleEfficiency;
    private readonly Ratio _suctionEfficiency;
    private readonly IEjector _sut;

    public EjectorTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _nozzleEfficiency = 0.9.DecimalFractions();
        _suctionEfficiency = 0.9.DecimalFractions();
        _diffuserEfficiency = 0.8.DecimalFractions();
        _sut = new Ejector(_nozzleEfficiency, _suctionEfficiency, _diffuserEfficiency);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(100.0)]
    public static void Ejector_WrongEfficiencySingle_ThrowsValidationException(
        double isentropicEfficiency
    )
    {
        Action action = () => _ = new Ejector(isentropicEfficiency.Percent());
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Isentropic efficiency of the nozzle should be in (0;100) %!*");
    }

    [Theory]
    [InlineData(0.0, 90.0, 80.0, "Isentropic efficiency of the nozzle should be in (0;100) %!")]
    [InlineData(100.0, 90.0, 80.0, "Isentropic efficiency of the nozzle should be in (0;100) %!")]
    [InlineData(
        90.0,
        0.0,
        80.0,
        "Isentropic efficiency of the suction section should be in (0;100) %!"
    )]
    [InlineData(
        90.0,
        100.0,
        80.0,
        "Isentropic efficiency of the suction section should be in (0;100) %!"
    )]
    [InlineData(90.0, 90.0, 0.0, "Isentropic efficiency of the diffuser should be in (0;100) %!")]
    [InlineData(90.0, 90.0, 100.0, "Isentropic efficiency of the diffuser should be in (0;100) %!")]
    public static void Ejector_WrongEfficiencyAny_ThrowsValidationException(
        double nozzleIsentropicEfficiency,
        double suctionIsentropicEfficiency,
        double diffuserIsentropicEfficiency,
        string message
    )
    {
        Action action = () =>
            _ = new Ejector(
                nozzleIsentropicEfficiency.Percent(),
                suctionIsentropicEfficiency.Percent(),
                diffuserIsentropicEfficiency.Percent()
            );
        action.Should().Throw<ValidationException>().WithMessage($"*{message}*");
    }

    [Fact]
    public void NozzleEfficiency_Always_ReturnsEnteredValueInPercents()
    {
        _sut.NozzleEfficiency.Equals(_nozzleEfficiency, _comparison.Tolerance.Percent())
            .Should()
            .BeTrue();
        _sut.NozzleEfficiency.Unit.Should().Be(RatioUnit.Percent);
    }

    [Fact]
    public void SuctionEfficiency_Always_ReturnsEnteredValueInPercents()
    {
        _sut.SuctionEfficiency.Equals(_suctionEfficiency, _comparison.Tolerance.Percent())
            .Should()
            .BeTrue();
        _sut.SuctionEfficiency.Unit.Should().Be(RatioUnit.Percent);
    }

    [Fact]
    public void DiffuserEfficiency_Always_ReturnsEnteredValueInPercents()
    {
        _sut.DiffuserEfficiency.Equals(_diffuserEfficiency, _comparison.Tolerance.Percent())
            .Should()
            .BeTrue();
        _sut.DiffuserEfficiency.Unit.Should().Be(RatioUnit.Percent);
    }

    [Fact]
    public void Equals_Same_ReturnsTrue()
    {
        var origin = new Ejector(90.Percent(), 90.Percent(), 90.Percent());
        var same = new Ejector(90.Percent());
        origin.Should().Be(same);
    }
}
