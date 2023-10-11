namespace VCRC.Tests;

public class VCRCWithRecuperatorSubcriticalTests
    : IClassFixture<ComparisonFixture>,
        IClassFixture<SubcriticalVCRCFixture<IVCRCWithRecuperator>>
{
    private readonly ComparisonFixture _comparison;
    private readonly SubcriticalVCRCFixture<IVCRCWithRecuperator> _fixture;

    public VCRCWithRecuperatorSubcriticalTests(
        ComparisonFixture comparison,
        SubcriticalVCRCFixture<IVCRCWithRecuperator> fixture
    )
    {
        _comparison = comparison;
        _fixture = fixture;
    }

    [Fact]
    public void VCRCWithRecuperator_WrongTemperatureDifferenceAtRecuperatorHotSide_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithRecuperator(
                _fixture.Evaporator,
                new Recuperator(TemperatureDelta.FromKelvins(49)),
                _fixture.Compressor,
                _fixture.Condenser
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*Too high temperature difference "
                    + "at the recuperator 'hot' side!*"
            );
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
    public void Point2_Always_ReturnsRecuperatorColdOutlet()
    {
        _fixture.Instance.Point2
            .Should()
            .Be(
                _fixture.Instance.Point1.HeatingTo(
                    _fixture.Instance.Point4.Temperature
                        - _fixture.Recuperator.TemperatureDifference
                )
            );
        _fixture.Instance.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point3s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        _fixture.Instance.Point3s
            .Should()
            .Be(
                _fixture.Instance.Point2.IsentropicCompressionTo(
                    _fixture.Condenser.Pressure
                )
            );
        _fixture.Instance.Point3s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_Always_ReturnsCompressionStageDischarge()
    {
        _fixture.Instance.Point3
            .Should()
            .Be(
                _fixture.Instance.Point2.CompressionTo(
                    _fixture.Condenser.Pressure,
                    _fixture.Compressor.Efficiency
                )
            );
        _fixture.Instance.Point3.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point4_ForThisCase_ReturnsCondenserOutlet()
    {
        _fixture.Instance.Point4.Should().Be(_fixture.Condenser.Outlet);
        _fixture.Instance.Point4.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point5_Always_ReturnsRecuperatorHotOutlet()
    {
        _fixture.Instance.Point5
            .Should()
            .Be(
                _fixture.Instance.Point4.CoolingTo(
                    _fixture.Instance.Point4.Enthalpy
                        - (
                            _fixture.Instance.Point2.Enthalpy
                            - _fixture.Instance.Point1.Enthalpy
                        )
                )
            );
        _fixture.Instance.Point5.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point6_Always_ReturnsEvaporatorInlet()
    {
        _fixture.Instance.Point6
            .Should()
            .Be(
                _fixture.Instance.Point5.IsenthalpicExpansionTo(
                    _fixture.Evaporator.Pressure
                )
            );
        _fixture.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _fixture.Instance.IsentropicSpecificWork
            .Equals(
                _fixture.Instance.Point3s.Enthalpy
                    - _fixture.Instance.Point2.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        _fixture.Instance.SpecificWork
            .Equals(
                _fixture.Instance.IsentropicSpecificWork
                    / _fixture.Compressor.Efficiency.DecimalFractions,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        _fixture.Instance.SpecificCoolingCapacity
            .Equals(
                _fixture.Instance.Point1.Enthalpy
                    - _fixture.Instance.Point6.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        _fixture.Instance.SpecificHeatingCapacity
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
        _fixture.Instance.EER
            .Should()
            .Be(
                _fixture.Instance.SpecificCoolingCapacity
                    / _fixture.Instance.SpecificWork
            );
        _fixture.Instance.EER
            .Should()
            .BeApproximately(4.201006672315493, _comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        _fixture.Instance.COP
            .Should()
            .Be(
                _fixture.Instance.SpecificHeatingCapacity
                    / _fixture.Instance.SpecificWork
            );
        _fixture.Instance.COP
            .Should()
            .BeApproximately(5.201006672315493, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout24() =>
        _fixture.AnalysisResult.ThermodynamicPerfection.Percent
            .Should()
            .BeApproximately(24.529319398716595, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout24() =>
        _fixture.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should()
            .BeApproximately(24.529319398716606, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _fixture.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout27() =>
        _fixture.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should()
            .BeApproximately(27.273640321252557, _comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        _fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        _fixture.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should()
            .BeApproximately(6.141707411883935, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout20() =>
        _fixture.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(20.545203060576515, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        _fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(1.510129807570391, _comparison.Tolerance);

    [Fact]
    public void EconomizerEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should()
            .Be(0);

    [Fact]
    public void TestMixingEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.MixingEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        _fixture.AnalysisResult.AnalysisRelativeError.Percent
            .Should()
            .BeApproximately(4.099022440185992e-14, _comparison.Tolerance);
        _fixture.AnalysisResult
            .Sum()
            .Percent.Should()
            .BeApproximately(100, _comparison.Tolerance);
    }
}
