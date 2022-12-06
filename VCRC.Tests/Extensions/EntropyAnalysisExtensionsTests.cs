using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class EntropyAnalysisExtensionsTests
{
    public EntropyAnalysisExtensionsTests()
    {
        Indoor = Enumerable.Range(18, 5)
            .Select(i => i.DegreesCelsius()).ToList();
        IndoorExtended = Enumerable.Range(18, 6)
            .Select(i => i.DegreesCelsius()).ToList();
        Outdoor = Enumerable.Range(36, 5)
            .Select(i => i.DegreesCelsius()).ToList();
        Cycles = Enumerable.Range(0, Indoor.Count)
            .Select(i => CreateVCRC(Indoor[i], Outdoor[i])).ToList();
    }

    private List<Temperature> Indoor { get; }

    private List<Temperature> Outdoor { get; }

    private List<IEntropyAnalysable> Cycles { get; }

    private List<Temperature> IndoorExtended { get; }

    [Fact]
    public void EntropyAnalysisInRange_ListsOfTemperaturesWithDifferentCount_ThrowsArgumentException()
    {
        Action action = () => Cycles.EntropyAnalysis(IndoorExtended, Outdoor);
        action.Should().Throw<ArgumentException>()
            .WithMessage("The lists should have the same length!");
    }

    [Fact]
    public void EntropyAnalysisInRange_Always_ReturnsAveragedEntropyAnalysisResult() =>
        Cycles.EntropyAnalysis(Indoor, Outdoor).Should().Be(
            Cycles.Select((c, i) => c.EntropyAnalysis(Indoor[i], Outdoor[i])).ToList().Average());

    private static IEntropyAnalysable CreateVCRC(Temperature indoor, Temperature outdoor)
    {
        const FluidsList refrigerantName = FluidsList.R32;
        var evaporator = new Evaporator(refrigerantName, indoor - TemperatureDelta.FromKelvins(7),
            TemperatureDelta.FromKelvins(5));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(refrigerantName, outdoor + TemperatureDelta.FromKelvins(10),
            TemperatureDelta.FromKelvins(3));
        return new SimpleVCRC(evaporator, compressor, condenser);
    }
}