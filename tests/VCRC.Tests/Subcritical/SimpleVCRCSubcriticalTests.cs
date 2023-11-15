using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class SimpleVCRCSubcriticalTests(
    ComparisonFixture comparison,
    SubcriticalVCRCFixture<ISimpleVCRC> fixture
)
    : IClassFixture<ComparisonFixture>,
        IClassFixture<SubcriticalVCRCFixture<ISimpleVCRC>>
{
    [Theory]
    [InlineData(5, 4)]
    [InlineData(41, 40)]
    public void SimpleVCRC_WrongTemperatures_ThrowsValidationException(
        double evaporatingTemperature,
        double condensingTemperature
    )
    {
        Action action = () =>
            _ = new SimpleVCRC(
                new Evaporator(
                    fixture.Refrigerant.Name,
                    evaporatingTemperature.DegreesCelsius(),
                    fixture.Evaporator.Superheat
                ),
                fixture.Compressor,
                new Condenser(
                    fixture.Refrigerant.Name,
                    condensingTemperature.DegreesCelsius(),
                    fixture.Condenser.Subcooling
                )
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*Condensing temperature "
                    + "should be greater than evaporating temperature!*"
            );
    }

    [Fact]
    public void SimpleVCRC_WrongRefrigerants_ThrowsValidationException()
    {
        Action action = () =>
            _ = new SimpleVCRC(
                new Evaporator(
                    FluidsList.R22,
                    fixture.Evaporator.Temperature,
                    fixture.Evaporator.Superheat
                ),
                fixture.Compressor,
                fixture.Condenser
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Only one refrigerant should be selected!*");
    }

    [Theory]
    [InlineData(18, 18, "Indoor and outdoor temperatures should not be equal!")]
    [InlineData(
        13,
        35,
        "Wrong temperature difference in the evaporator! "
            + "Increase 'cold' source temperature."
    )]
    [InlineData(
        18,
        42,
        "Wrong temperature difference in the condenser or gas cooler! "
            + "Decrease 'hot' source temperature."
    )]
    public void EntropyAnalysis_WrongTemperatures_ThrowsValidationException(
        double indoor,
        double outdoor,
        string message
    )
    {
        IEntropyAnalysable vcrc = fixture.Instance;
        Action action = () =>
            vcrc.EntropyAnalysis(
                indoor.DegreesCelsius(),
                outdoor.DegreesCelsius()
            );
        action.Should().Throw<ArgumentException>().WithMessage($"*{message}*");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        fixture.Instance.Evaporator.Should().Be(fixture.Evaporator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        fixture.Instance.Compressor.Should().Be(fixture.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsEnteredValue() =>
        fixture.Instance.Condenser.Should().Be(fixture.Condenser);

    [Fact]
    public void GasCooler_ForThisCase_ReturnsNull() =>
        fixture.Instance.GasCooler.Should().BeNull();

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        fixture.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        fixture.Instance.Point1.Should().Be(fixture.Evaporator.Outlet);
        fixture.Instance.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point2s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance
            .Point2s
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point1
                    .IsentropicCompressionTo(fixture.Condenser.Pressure)
            );
        fixture.Instance.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point2_Always_ReturnsCompressionStageDischarge()
    {
        fixture
            .Instance
            .Point2
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point1
                    .CompressionTo(
                        fixture.Condenser.Pressure,
                        fixture.Compressor.Efficiency
                    )
            );
        fixture.Instance.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_ForThisCase_ReturnsCondenserOutlet()
    {
        fixture.Instance.Point3.Should().Be(fixture.Condenser.Outlet);
        fixture.Instance.Point3.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point4_Always_ReturnsEvaporatorInlet()
    {
        fixture
            .Instance
            .Point4
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point3
                    .IsenthalpicExpansionTo(fixture.Evaporator.Pressure)
            );
        fixture.Instance.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        fixture
            .Instance
            .IsentropicSpecificWork
            .Equals(
                fixture.Instance.Point2s.Enthalpy
                    - fixture.Instance.Point1.Enthalpy,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        fixture
            .Instance
            .SpecificWork
            .Equals(
                fixture.Instance.IsentropicSpecificWork
                    / fixture.Compressor.Efficiency.DecimalFractions,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        fixture
            .Instance
            .SpecificCoolingCapacity
            .Equals(
                fixture.Instance.Point1.Enthalpy
                    - fixture.Instance.Point4.Enthalpy,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        fixture
            .Instance
            .SpecificHeatingCapacity
            .Equals(
                fixture.Instance.Point2.Enthalpy
                    - fixture.Instance.Point3.Enthalpy,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        fixture
            .Instance
            .EER
            .Should()
            .Be(
                fixture.Instance.SpecificCoolingCapacity
                    / fixture.Instance.SpecificWork
            );
        fixture
            .Instance
            .EER
            .Should()
            .BeApproximately(4.326011919496399, comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        fixture
            .Instance
            .COP
            .Should()
            .Be(
                fixture.Instance.SpecificHeatingCapacity
                    / fixture.Instance.SpecificWork
            );
        fixture
            .Instance
            .COP
            .Should()
            .BeApproximately(5.326011919496398, comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout25() =>
        fixture
            .AnalysisResult
            .ThermodynamicPerfection
            .Percent
            .Should()
            .BeApproximately(25.259214367658867, comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout25() =>
        fixture
            .AnalysisResult
            .MinSpecificWorkRatio
            .Percent
            .Should()
            .BeApproximately(25.259214367658878, comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        fixture
            .AnalysisResult
            .CompressorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(20, comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        fixture
            .AnalysisResult
            .CondenserEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(21.46877321647669, comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout12() =>
        fixture
            .AnalysisResult
            .ExpansionValvesEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(12.141693490520616, comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        fixture
            .AnalysisResult
            .EvaporatorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(21.13031892534382, comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        fixture
            .AnalysisResult
            .RecuperatorEnergyLossRatio
            .Percent
            .Should()
            .Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.EconomizerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void TestMixingEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.MixingEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        fixture
            .AnalysisResult
            .AnalysisRelativeError
            .Percent
            .Should()
            .BeApproximately(6.236322468275591e-14, comparison.Tolerance);
        fixture
            .AnalysisResult
            .Sum()
            .Percent
            .Should()
            .BeApproximately(100, comparison.Tolerance);
    }
}
