using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class SimpleVCRCSubcriticalTests
    : IClassFixture<ComparisonFixture>,
        IClassFixture<SubcriticalVCRCFixture<ISimpleVCRC>>
{
    private readonly ComparisonFixture _comparison;
    private readonly SubcriticalVCRCFixture<ISimpleVCRC> _fixture;

    public SimpleVCRCSubcriticalTests(
        ComparisonFixture comparison,
        SubcriticalVCRCFixture<ISimpleVCRC> fixture
    )
    {
        _comparison = comparison;
        _fixture = fixture;
    }

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
                    _fixture.Refrigerant.Name,
                    evaporatingTemperature.DegreesCelsius(),
                    _fixture.Evaporator.Superheat
                ),
                _fixture.Compressor,
                new Condenser(
                    _fixture.Refrigerant.Name,
                    condensingTemperature.DegreesCelsius(),
                    _fixture.Condenser.Subcooling
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
                    _fixture.Evaporator.Temperature,
                    _fixture.Evaporator.Superheat
                ),
                _fixture.Compressor,
                _fixture.Condenser
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
        IEntropyAnalysable vcrc = _fixture.Instance;
        Action action = () =>
            vcrc.EntropyAnalysis(
                indoor.DegreesCelsius(),
                outdoor.DegreesCelsius()
            );
        action.Should().Throw<ArgumentException>().WithMessage($"*{message}*");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        _fixture.Instance.Evaporator.Should().Be(_fixture.Evaporator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        _fixture.Instance.Compressor.Should().Be(_fixture.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsEnteredValue() =>
        _fixture.Instance.Condenser.Should().Be(_fixture.Condenser);

    [Fact]
    public void GasCooler_ForThisCase_ReturnsNull() =>
        _fixture.Instance.GasCooler.Should().BeNull();

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        _fixture.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        _fixture.Instance.Point1.Should().Be(_fixture.Evaporator.Outlet);
        _fixture.Instance.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point2s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        _fixture
            .Instance
            .Point2s
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point1
                    .IsentropicCompressionTo(_fixture.Condenser.Pressure)
            );
        _fixture.Instance.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point2_Always_ReturnsCompressionStageDischarge()
    {
        _fixture
            .Instance
            .Point2
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point1
                    .CompressionTo(
                        _fixture.Condenser.Pressure,
                        _fixture.Compressor.Efficiency
                    )
            );
        _fixture.Instance.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_ForThisCase_ReturnsCondenserOutlet()
    {
        _fixture.Instance.Point3.Should().Be(_fixture.Condenser.Outlet);
        _fixture.Instance.Point3.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point4_Always_ReturnsEvaporatorInlet()
    {
        _fixture
            .Instance
            .Point4
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point3
                    .IsenthalpicExpansionTo(_fixture.Evaporator.Pressure)
            );
        _fixture.Instance.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _fixture
            .Instance
            .IsentropicSpecificWork
            .Equals(
                _fixture.Instance.Point2s.Enthalpy
                    - _fixture.Instance.Point1.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        _fixture
            .Instance
            .SpecificWork
            .Equals(
                _fixture.Instance.IsentropicSpecificWork
                    / _fixture.Compressor.Efficiency.DecimalFractions,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        _fixture
            .Instance
            .SpecificCoolingCapacity
            .Equals(
                _fixture.Instance.Point1.Enthalpy
                    - _fixture.Instance.Point4.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        _fixture
            .Instance
            .SpecificHeatingCapacity
            .Equals(
                _fixture.Instance.Point2.Enthalpy
                    - _fixture.Instance.Point3.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        _fixture
            .Instance
            .EER
            .Should()
            .Be(
                _fixture.Instance.SpecificCoolingCapacity
                    / _fixture.Instance.SpecificWork
            );
        _fixture
            .Instance
            .EER
            .Should()
            .BeApproximately(4.326011919496399, _comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        _fixture
            .Instance
            .COP
            .Should()
            .Be(
                _fixture.Instance.SpecificHeatingCapacity
                    / _fixture.Instance.SpecificWork
            );
        _fixture
            .Instance
            .COP
            .Should()
            .BeApproximately(5.326011919496398, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout25() =>
        _fixture
            .AnalysisResult
            .ThermodynamicPerfection
            .Percent
            .Should()
            .BeApproximately(25.259214367658867, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout25() =>
        _fixture
            .AnalysisResult
            .MinSpecificWorkRatio
            .Percent
            .Should()
            .BeApproximately(25.259214367658878, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _fixture
            .AnalysisResult
            .CompressorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        _fixture
            .AnalysisResult
            .CondenserEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(21.46877321647669, _comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        _fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout12() =>
        _fixture
            .AnalysisResult
            .ExpansionValvesEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(12.141693490520616, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        _fixture
            .AnalysisResult
            .EvaporatorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(21.13031892534382, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        _fixture
            .AnalysisResult
            .RecuperatorEnergyLossRatio
            .Percent
            .Should()
            .Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_Always_Returns0() =>
        _fixture
            .AnalysisResult
            .EconomizerEnergyLossRatio
            .Percent
            .Should()
            .Be(0);

    [Fact]
    public void TestMixingEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.MixingEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        _fixture
            .AnalysisResult
            .AnalysisRelativeError
            .Percent
            .Should()
            .BeApproximately(6.236322468275591e-14, _comparison.Tolerance);
        _fixture
            .AnalysisResult
            .Sum()
            .Percent
            .Should()
            .BeApproximately(100, _comparison.Tolerance);
    }
}
