using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class CloneExtensionsTests
{
    public CloneExtensionsTests() =>
        Refrigerant = new Refrigerant(FluidsList.R718);

    private Refrigerant Refrigerant { get; }

    [Fact]
    public void Clone_Always_ReturnsNewInstanceWithSameState()
    {
        var origin = Refrigerant.WithState(Input.Pressure(1.Atmospheres()),
            Input.Temperature(20.DegreesCelsius()));
        var clone = origin.Clone();
        clone.Should().Be(origin);
        clone.Update(Input.Pressure(1.Atmospheres()),
            Input.Temperature(30.DegreesCelsius()));
        clone.Should().NotBe(origin);
    }
}