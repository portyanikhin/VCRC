namespace VCRC.Tests;

public class VCRCWithRecuperatorTranscriticalTests
    : IClassFixture<ComparisonFixture>,
        IClassFixture<TranscriticalVCRCFixture<VCRCWithRecuperator>>
{
    private readonly ComparisonFixture _comparison;
    private readonly TranscriticalVCRCFixture<VCRCWithRecuperator> _vcrc;

    public VCRCWithRecuperatorTranscriticalTests(
        ComparisonFixture comparison,
        TranscriticalVCRCFixture<VCRCWithRecuperator> vcrc
    )
    {
        _comparison = comparison;
        _vcrc = vcrc;
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Evaporator.Should().Be(_vcrc.Evaporator);

    [Fact]
    public void Recuperator_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Recuperator.Should().Be(_vcrc.Recuperator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Compressor.Should().Be(_vcrc.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsNull() =>
        _vcrc.Instance.Condenser.Should().BeNull();

    [Fact]
    public void GasCooler_ForThisCase_ReturnsEnteredValue() =>
        _vcrc.Instance.GasCooler.Should().Be(_vcrc.GasCooler);

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsTrue() =>
        _vcrc.Instance.IsTranscritical.Should().BeTrue();

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        _vcrc.Instance.Point1.Should().Be(_vcrc.Evaporator.Outlet);
        _vcrc.Instance.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsRecuperatorColdOutlet()
    {
        _vcrc.Instance.Point2
            .Should()
            .Be(
                _vcrc.Instance.Point1.HeatingTo(
                    _vcrc.Instance.Point4.Temperature
                        - _vcrc.Recuperator.TemperatureDifference
                )
            );
        _vcrc.Instance.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point3s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        _vcrc.Instance.Point3s
            .Should()
            .Be(
                _vcrc.Instance.Point2.IsentropicCompressionTo(
                    _vcrc.GasCooler.Pressure
                )
            );
        _vcrc.Instance.Point3s.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point3_Always_ReturnsCompressionStageDischarge()
    {
        _vcrc.Instance.Point3
            .Should()
            .Be(
                _vcrc.Instance.Point2.CompressionTo(
                    _vcrc.GasCooler.Pressure,
                    _vcrc.Compressor.Efficiency
                )
            );
        _vcrc.Instance.Point3.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point4_ForThisCase_ReturnsGasCoolerOutlet()
    {
        _vcrc.Instance.Point4.Should().Be(_vcrc.GasCooler.Outlet);
        _vcrc.Instance.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point5_Always_ReturnsRecuperatorHotOutlet()
    {
        _vcrc.Instance.Point5
            .Should()
            .Be(
                _vcrc.Instance.Point4.CoolingTo(
                    _vcrc.Instance.Point4.Enthalpy
                        - (
                            _vcrc.Instance.Point2.Enthalpy
                            - _vcrc.Instance.Point1.Enthalpy
                        )
                )
            );
        _vcrc.Instance.Point5.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point6_Always_ReturnsEvaporatorInlet()
    {
        _vcrc.Instance.Point6
            .Should()
            .Be(
                _vcrc.Instance.Point5.IsenthalpicExpansionTo(
                    _vcrc.Evaporator.Pressure
                )
            );
        _vcrc.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _vcrc.Instance.IsentropicSpecificWork
            .Equals(
                _vcrc.Instance.Point3s.Enthalpy
                    - _vcrc.Instance.Point2.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        _vcrc.Instance.SpecificWork
            .Equals(
                _vcrc.Instance.IsentropicSpecificWork
                    / _vcrc.Compressor.Efficiency.DecimalFractions,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        _vcrc.Instance.SpecificCoolingCapacity
            .Equals(
                _vcrc.Instance.Point1.Enthalpy - _vcrc.Instance.Point6.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        _vcrc.Instance.SpecificHeatingCapacity
            .Equals(
                _vcrc.Instance.Point3.Enthalpy - _vcrc.Instance.Point4.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        _vcrc.Instance.EER
            .Should()
            .Be(
                _vcrc.Instance.SpecificCoolingCapacity
                    / _vcrc.Instance.SpecificWork
            );
        _vcrc.Instance.EER
            .Should()
            .BeApproximately(2.711892365925208, _comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        _vcrc.Instance.COP
            .Should()
            .Be(
                _vcrc.Instance.SpecificHeatingCapacity
                    / _vcrc.Instance.SpecificWork
            );
        _vcrc.Instance.COP
            .Should()
            .BeApproximately(3.7118923659252077, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout15() =>
        _vcrc.AnalysisResult.ThermodynamicPerfection.Percent
            .Should()
            .BeApproximately(15.834508061387101, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout15() =>
        _vcrc.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should()
            .BeApproximately(15.83450806138709, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _vcrc.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_Returns0() =>
        _vcrc.AnalysisResult.CondenserEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_ReturnsAbout29() =>
        _vcrc.AnalysisResult.GasCoolerEnergyLossRatio.Percent
            .Should()
            .BeApproximately(29.670513651007557, _comparison.Tolerance);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout19() =>
        _vcrc.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should()
            .BeApproximately(19.10155564959979, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout13() =>
        _vcrc.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(13.037369549556171, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_ForThisCase_ReturnsAbout2() =>
        _vcrc.AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(2.356053088449407, _comparison.Tolerance);

    [Fact]
    public void EconomizerEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.EconomizerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void TestMixingEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.MixingEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        _vcrc.AnalysisResult.AnalysisRelativeError.Percent
            .Should()
            .BeApproximately(3.7834531981960063e-14, _comparison.Tolerance);
        _vcrc.AnalysisResult
            .Sum()
            .Percent.Should()
            .BeApproximately(100, _comparison.Tolerance);
    }
}
