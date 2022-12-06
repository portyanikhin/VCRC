namespace VCRC.Tests;

public class EjectorTests
{
    private static readonly Ratio NozzleEfficiency = 0.9.DecimalFractions();
    private static readonly Ratio SuctionEfficiency = 0.9.DecimalFractions();
    private static readonly Ratio DiffuserEfficiency = 0.8.DecimalFractions();

    public EjectorTests() =>
        Ejector = new Ejector(NozzleEfficiency, SuctionEfficiency, DiffuserEfficiency);

    private Ejector Ejector { get; }

    [Theory]
    [InlineData(0.0)]
    [InlineData(100.0)]
    public static void Ejector_WrongEfficiencySingle_ThrowsValidationException(double isentropicEfficiency)
    {
        Action action = () => _ = new Ejector(isentropicEfficiency.Percent());
        action.Should().Throw<ValidationException>()
            .WithMessage("*Isentropic efficiency of the nozzle should be in (0;100) %!*");
    }

    [Theory]
    [InlineData(0.0, 90.0, 80.0, "Isentropic efficiency of the nozzle should be in (0;100) %!")]
    [InlineData(100.0, 90.0, 80.0, "Isentropic efficiency of the nozzle should be in (0;100) %!")]
    [InlineData(90.0, 0.0, 80.0, "Isentropic efficiency of the suction section should be in (0;100) %!")]
    [InlineData(90.0, 100.0, 80.0, "Isentropic efficiency of the suction section should be in (0;100) %!")]
    [InlineData(90.0, 90.0, 0.0, "Isentropic efficiency of the diffuser should be in (0;100) %!")]
    [InlineData(90.0, 90.0, 100.0, "Isentropic efficiency of the diffuser should be in (0;100) %!")]
    public static void Ejector_WrongEfficiencyAny_ThrowsValidationException(
        double nozzleIsentropicEfficiency,
        double suctionIsentropicEfficiency,
        double diffuserIsentropicEfficiency,
        string message)
    {
        Action action = () => _ = new Ejector(
            nozzleIsentropicEfficiency.Percent(),
            suctionIsentropicEfficiency.Percent(),
            diffuserIsentropicEfficiency.Percent());
        action.Should().Throw<ValidationException>().WithMessage($"*{message}*");
    }

    [Fact]
    public void NozzleEfficiency_Always_ReturnsEnteredValueInPercents()
    {
        Ejector.NozzleEfficiency.Should().Be(NozzleEfficiency);
        Ejector.NozzleEfficiency.Unit.Should().Be(RatioUnit.Percent);
    }

    [Fact]
    public void SuctionEfficiency_Always_ReturnsEnteredValueInPercents()
    {
        Ejector.SuctionEfficiency.Should().Be(SuctionEfficiency);
        Ejector.SuctionEfficiency.Unit.Should().Be(RatioUnit.Percent);
    }

    [Fact]
    public void DiffuserEfficiency_Always_ReturnsEnteredValueInPercents()
    {
        Ejector.DiffuserEfficiency.Should().Be(DiffuserEfficiency);
        Ejector.DiffuserEfficiency.Unit.Should().Be(RatioUnit.Percent);
    }

    [Fact]
    public void Equals_Same_ReturnsTrue()
    {
        var origin = new Ejector(90.Percent(), 90.Percent(), 90.Percent());
        var same = new Ejector(90.Percent());
        origin.Should().Be(same);
    }
}