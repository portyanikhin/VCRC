namespace VCRC.Tests;

public class VCRCWithRecuperatorSubcriticalTests
    : IClassFixture<ComparisonFixture>,
        IClassFixture<SubcriticalVCRCFixture<VCRCWithRecuperator>>
{
    private readonly ComparisonFixture _comparison;
    private readonly SubcriticalVCRCFixture<VCRCWithRecuperator> _vcrc;

    public VCRCWithRecuperatorSubcriticalTests(
        ComparisonFixture comparison,
        SubcriticalVCRCFixture<VCRCWithRecuperator> vcrc
    )
    {
        _comparison = comparison;
        _vcrc = vcrc;
    }

    [Fact]
    public void VCRCWithRecuperator_WrongTemperatureDifferenceAtRecuperatorHotSide_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithRecuperator(
                _vcrc.Evaporator,
                new Recuperator(TemperatureDelta.FromKelvins(49)),
                _vcrc.Compressor,
                _vcrc.Condenser
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*Too high temperature difference at recuperator 'hot' side!*"
            );
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
    public void Condenser_ForThisCase_ReturnsEnteredValue() =>
        _vcrc.Instance.Condenser.Should().Be(_vcrc.Condenser);

    [Fact]
    public void GasCooler_ForThisCase_ReturnsNull() =>
        _vcrc.Instance.GasCooler.Should().BeNull();

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        _vcrc.Instance.IsTranscritical.Should().BeFalse();

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
        _vcrc.Instance.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point3s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        _vcrc.Instance.Point3s
            .Should()
            .Be(
                _vcrc.Instance.Point2.IsentropicCompressionTo(
                    _vcrc.Condenser.Pressure
                )
            );
        _vcrc.Instance.Point3s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_Always_ReturnsCompressionStageDischarge()
    {
        _vcrc.Instance.Point3
            .Should()
            .Be(
                _vcrc.Instance.Point2.CompressionTo(
                    _vcrc.Condenser.Pressure,
                    _vcrc.Compressor.Efficiency
                )
            );
        _vcrc.Instance.Point3.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point4_ForThisCase_ReturnsCondenserOutlet()
    {
        _vcrc.Instance.Point4.Should().Be(_vcrc.Condenser.Outlet);
        _vcrc.Instance.Point4.Phase.Should().Be(Phases.Liquid);
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
        _vcrc.Instance.Point5.Phase.Should().Be(Phases.Liquid);
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
            .BeApproximately(4.201006672315493, _comparison.Tolerance);
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
            .BeApproximately(5.201006672315493, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout24() =>
        _vcrc.AnalysisResult.ThermodynamicPerfection.Percent
            .Should()
            .BeApproximately(24.529319398716595, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout24() =>
        _vcrc.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should()
            .BeApproximately(24.529319398716606, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _vcrc.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout27() =>
        _vcrc.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should()
            .BeApproximately(27.273640321252557, _comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        _vcrc.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        _vcrc.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should()
            .BeApproximately(6.141707411883935, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout20() =>
        _vcrc.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(20.545203060576515, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        _vcrc.AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(1.510129807570391, _comparison.Tolerance);

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
            .BeApproximately(4.099022440185992e-14, _comparison.Tolerance);
        _vcrc.AnalysisResult
            .Sum()
            .Percent.Should()
            .BeApproximately(100, _comparison.Tolerance);
    }
}
