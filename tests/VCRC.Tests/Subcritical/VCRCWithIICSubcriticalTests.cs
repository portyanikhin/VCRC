namespace VCRC.Tests;

public class VCRCWithIICSubcriticalTests
    : IClassFixture<ComparisonFixture>,
        IClassFixture<SubcriticalVCRCFixture<IVCRCWithIIC>>
{
    private readonly ComparisonFixture _comparison;
    private readonly SubcriticalVCRCFixture<IVCRCWithIIC> _fixture;

    public VCRCWithIICSubcriticalTests(
        ComparisonFixture comparison,
        SubcriticalVCRCFixture<IVCRCWithIIC> fixture
    )
    {
        _comparison = comparison;
        _fixture = fixture;
    }

    [Fact]
    public void VCRCWithIIC_WrongRefrigerant_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithIIC(
                new Evaporator(
                    FluidsList.R407C,
                    _fixture.Evaporator.Temperature,
                    _fixture.Evaporator.Superheat
                ),
                _fixture.Compressor,
                new Condenser(
                    FluidsList.R407C,
                    _fixture.Condenser.Temperature,
                    _fixture.Condenser.Subcooling
                )
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*Refrigerant should be "
                    + "a single component or an azeotropic blend!*"
            );
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
    public void Point2s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        _fixture
            .Instance
            .Point2s
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point1
                    .IsentropicCompressionTo(
                        _fixture.Instance.IntermediatePressure
                    )
            );
        _fixture.Instance.Point2s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsFirstCompressionStageDischarge()
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
                        _fixture.Instance.IntermediatePressure,
                        _fixture.Compressor.Efficiency
                    )
            );
        _fixture.Instance.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point3_Always_ReturnsSecondCompressionStageSuction()
    {
        _fixture
            .Instance
            .Point3
            .Should()
            .Be(
                _fixture
                    .Refrigerant
                    .Mixing(
                        _fixture.Instance.EvaporatorSpecificMassFlow,
                        _fixture.Instance.Point2,
                        _fixture.Instance.HeatReleaserSpecificMassFlow
                            - _fixture.Instance.EvaporatorSpecificMassFlow,
                        _fixture.Instance.Point7
                    )
            );
        _fixture.Instance.Point3.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point4s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        _fixture
            .Instance
            .Point4s
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point3
                    .IsentropicCompressionTo(_fixture.Condenser.Pressure)
            );
        _fixture.Instance.Point4s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point4_Always_ReturnsSecondCompressionStageDischarge()
    {
        _fixture
            .Instance
            .Point4
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point3
                    .CompressionTo(
                        _fixture.Condenser.Pressure,
                        _fixture.Compressor.Efficiency
                    )
            );
        _fixture.Instance.Point4.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point5_ForThisCase_ReturnsCondenserOutlet()
    {
        _fixture.Instance.Point5.Should().Be(_fixture.Condenser.Outlet);
        _fixture.Instance.Point5.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point6_Always_ReturnsSeparatorInlet()
    {
        _fixture
            .Instance
            .Point6
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point5
                    .IsenthalpicExpansionTo(
                        _fixture.Instance.IntermediatePressure
                    )
            );
        _fixture.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point7_Always_ReturnsSeparatorVaporOutlet()
    {
        _fixture
            .Instance
            .Point7
            .Should()
            .Be(
                _fixture
                    .Refrigerant
                    .DewPointAt(_fixture.Instance.IntermediatePressure)
            );
        _fixture.Instance.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsSeparatorLiquidOutlet()
    {
        _fixture
            .Instance
            .Point8
            .Should()
            .Be(
                _fixture
                    .Refrigerant
                    .BubblePointAt(_fixture.Instance.IntermediatePressure)
            );
        _fixture.Instance.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point9_Always_ReturnsEvaporatorInlet()
    {
        _fixture
            .Instance
            .Point9
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point8
                    .IsenthalpicExpansionTo(_fixture.Evaporator.Pressure)
            );
        _fixture.Instance.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        _fixture
            .Instance
            .EvaporatorSpecificMassFlow
            .Equals(100.Percent(), _comparison.Tolerance.Percent())
            .Should()
            .BeTrue();
        _fixture
            .Instance
            .HeatReleaserSpecificMassFlow
            .Equals(
                _fixture.Instance.EvaporatorSpecificMassFlow
                    / (
                        1
                        - _fixture
                            .Instance
                            .Point6
                            .Quality!
                            .Value
                            .DecimalFractions
                    ),
                _comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
        _fixture
            .Instance
            .IntermediateSpecificMassFlow
            .Equals(
                _fixture.Instance.HeatReleaserSpecificMassFlow
                    - _fixture.Instance.EvaporatorSpecificMassFlow,
                _comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _fixture
            .Instance
            .IsentropicSpecificWork
            .Equals(
                _fixture.Instance.Point2s.Enthalpy
                    - _fixture.Instance.Point1.Enthalpy
                    + _fixture
                        .Instance
                        .HeatReleaserSpecificMassFlow
                        .DecimalFractions
                        * (
                            _fixture.Instance.Point4s.Enthalpy
                            - _fixture.Instance.Point3.Enthalpy
                        ),
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
                    - _fixture.Instance.Point9.Enthalpy,
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
                _fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                    * (
                        _fixture.Instance.Point4.Enthalpy
                        - _fixture.Instance.Point5.Enthalpy
                    ),
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
            .BeApproximately(4.591346929801504, _comparison.Tolerance);
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
            .BeApproximately(5.591346929801503, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout26() =>
        _fixture
            .AnalysisResult
            .ThermodynamicPerfection
            .Percent
            .Should()
            .BeApproximately(26.808482846170552, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout26() =>
        _fixture
            .AnalysisResult
            .MinSpecificWorkRatio
            .Percent
            .Should()
            .BeApproximately(26.681634761658042, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _fixture
            .AnalysisResult
            .CompressorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout22() =>
        _fixture
            .AnalysisResult
            .CondenserEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(22.953213501021036, _comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        _fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout7() =>
        _fixture
            .AnalysisResult
            .ExpansionValvesEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(7.656335268000471, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout22() =>
        _fixture
            .AnalysisResult
            .EvaporatorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(22.3568946179561, _comparison.Tolerance);

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
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        _fixture
            .AnalysisResult
            .MixingEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(0.3519218513643524, _comparison.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        _fixture
            .AnalysisResult
            .AnalysisRelativeError
            .Percent
            .Should()
            .BeApproximately(0.47541346565012926, _comparison.Tolerance);
        _fixture
            .AnalysisResult
            .Sum()
            .Percent
            .Should()
            .BeApproximately(100, _comparison.Tolerance);
    }
}
