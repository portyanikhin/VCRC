using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class EntropyAnalysisExtensionsTests
{
    private readonly IList<IEntropyAnalysable> _cycles;
    private readonly IList<Temperature> _indoor;
    private readonly IList<Temperature> _indoorExtended;
    private readonly IList<Temperature> _outdoor;

    public EntropyAnalysisExtensionsTests()
    {
        _indoor = Enumerable
            .Range(18, 5)
            .Select(i => i.DegreesCelsius())
            .ToList();
        _indoorExtended = Enumerable
            .Range(18, 6)
            .Select(i => i.DegreesCelsius())
            .ToList();
        _outdoor = Enumerable
            .Range(36, 5)
            .Select(i => i.DegreesCelsius())
            .ToList();
        _cycles = Enumerable
            .Range(0, _indoor.Count)
            .Select(i => CreateVCRC(_indoor[i], _outdoor[i]))
            .ToList();
    }

    [Fact]
    public void EntropyAnalysisInRange_ListsOfTemperaturesWithDifferentCount_ThrowsArgumentException()
    {
        Action action = () =>
            _cycles.EntropyAnalysis(_indoorExtended, _outdoor);
        action
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("The lists should have the same length!");
    }

    [Fact]
    public void EntropyAnalysisInRange_Always_ReturnsAveragedEntropyAnalysisResult() =>
        _cycles
            .EntropyAnalysis(_indoor, _outdoor)
            .Should()
            .Be(
                _cycles
                    .Select(
                        (cycle, i) =>
                            cycle.EntropyAnalysis(_indoor[i], _outdoor[i])
                    )
                    .ToList()
                    .Average()
            );

    private static IEntropyAnalysable CreateVCRC(
        Temperature indoor,
        Temperature outdoor
    )
    {
        const FluidsList refrigerantName = FluidsList.R32;
        var evaporator = new Evaporator(
            refrigerantName,
            indoor - TemperatureDelta.FromKelvins(7),
            TemperatureDelta.FromKelvins(5)
        );
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(
            refrigerantName,
            outdoor + TemperatureDelta.FromKelvins(10),
            TemperatureDelta.FromKelvins(3)
        );
        return new SimpleVCRC(evaporator, compressor, condenser);
    }
}
