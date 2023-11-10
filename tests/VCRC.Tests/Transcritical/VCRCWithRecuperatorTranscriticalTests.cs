namespace VCRC.Tests;

public class VCRCWithRecuperatorTranscriticalTests
    : IClassFixture<ComparisonFixture>,
        IClassFixture<TranscriticalVCRCFixture<IVCRCWithRecuperator>>
{
    private readonly ComparisonFixture _comparison;
    private readonly TranscriticalVCRCFixture<IVCRCWithRecuperator> _fixture;

    public VCRCWithRecuperatorTranscriticalTests(
        ComparisonFixture comparison,
        TranscriticalVCRCFixture<IVCRCWithRecuperator> fixture
    )
    {
        _comparison = comparison;
        _fixture = fixture;
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        _fixture.Instance.Evaporator.Should().Be(_fixture.Evaporator);

    [Fact]
    public void Recuperator_Always_ReturnsEnteredValue()
    {
        IHaveRecuperator sut = _fixture.Instance;
        sut.Recuperator.Should().Be(_fixture.Recuperator);
    }

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        _fixture.Instance.Compressor.Should().Be(_fixture.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsNull() =>
        _fixture.Instance.Condenser.Should().BeNull();

    [Fact]
    public void GasCooler_ForThisCase_ReturnsEnteredValue() =>
        _fixture.Instance.GasCooler.Should().Be(_fixture.GasCooler);

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsTrue() =>
        _fixture.Instance.IsTranscritical.Should().BeTrue();

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        _fixture.Instance.Point1.Should().Be(_fixture.Evaporator.Outlet);
        _fixture.Instance.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsRecuperatorColdOutlet()
    {
        _fixture
            .Instance
            .Point2
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point1
                    .HeatingTo(
                        _fixture.Instance.Point4.Temperature
                            - _fixture.Recuperator.TemperatureDifference
                    )
            );
        _fixture.Instance.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point3s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        _fixture
            .Instance
            .Point3s
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point2
                    .IsentropicCompressionTo(_fixture.GasCooler.Pressure)
            );
        _fixture.Instance.Point3s.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point3_Always_ReturnsCompressionStageDischarge()
    {
        _fixture
            .Instance
            .Point3
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point2
                    .CompressionTo(
                        _fixture.GasCooler.Pressure,
                        _fixture.Compressor.Efficiency
                    )
            );
        _fixture.Instance.Point3.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point4_ForThisCase_ReturnsGasCoolerOutlet()
    {
        _fixture.Instance.Point4.Should().Be(_fixture.GasCooler.Outlet);
        _fixture.Instance.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point5_Always_ReturnsRecuperatorHotOutlet()
    {
        _fixture
            .Instance
            .Point5
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point4
                    .CoolingTo(
                        _fixture.Instance.Point4.Enthalpy
                            - (
                                _fixture.Instance.Point2.Enthalpy
                                - _fixture.Instance.Point1.Enthalpy
                            )
                    )
            );
        _fixture.Instance.Point5.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point6_Always_ReturnsEvaporatorInlet()
    {
        _fixture
            .Instance
            .Point6
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point5
                    .IsenthalpicExpansionTo(_fixture.Evaporator.Pressure)
            );
        _fixture.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _fixture
            .Instance
            .IsentropicSpecificWork
            .Equals(
                _fixture.Instance.Point3s.Enthalpy
                    - _fixture.Instance.Point2.Enthalpy,
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
                    - _fixture.Instance.Point6.Enthalpy,
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
                _fixture.Instance.Point3.Enthalpy
                    - _fixture.Instance.Point4.Enthalpy,
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
            .BeApproximately(2.711892365925208, _comparison.Tolerance);
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
            .BeApproximately(3.7118923659252077, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout15() =>
        _fixture
            .AnalysisResult
            .ThermodynamicPerfection
            .Percent
            .Should()
            .BeApproximately(15.834508061387101, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout15() =>
        _fixture
            .AnalysisResult
            .MinSpecificWorkRatio
            .Percent
            .Should()
            .BeApproximately(15.83450806138709, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _fixture
            .AnalysisResult
            .CompressorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_Returns0() =>
        _fixture.AnalysisResult.CondenserEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_ReturnsAbout29() =>
        _fixture
            .AnalysisResult
            .GasCoolerEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(29.670513651007557, _comparison.Tolerance);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout19() =>
        _fixture
            .AnalysisResult
            .ExpansionValvesEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(19.10155564959979, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout13() =>
        _fixture
            .AnalysisResult
            .EvaporatorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(13.037369549556171, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_ForThisCase_ReturnsAbout2() =>
        _fixture
            .AnalysisResult
            .RecuperatorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(2.356053088449407, _comparison.Tolerance);

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
            .BeApproximately(3.7834531981960063e-14, _comparison.Tolerance);
        _fixture
            .AnalysisResult
            .Sum()
            .Percent
            .Should()
            .BeApproximately(100, _comparison.Tolerance);
    }
}
