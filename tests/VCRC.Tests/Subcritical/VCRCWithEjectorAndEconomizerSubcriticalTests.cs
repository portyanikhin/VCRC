namespace VCRC.Tests;

public class VCRCWithEjectorAndEconomizerSubcriticalTests
    : IClassFixture<ComparisonFixture>,
        IClassFixture<SubcriticalVCRCFixture<IVCRCWithEjectorAndEconomizer>>
{
    private readonly ComparisonFixture _comparison;
    private readonly SubcriticalVCRCFixture<IVCRCWithEjectorAndEconomizer> _fixture;

    public VCRCWithEjectorAndEconomizerSubcriticalTests(
        ComparisonFixture comparison,
        SubcriticalVCRCFixture<IVCRCWithEjectorAndEconomizer> fixture
    )
    {
        _comparison = comparison;
        _fixture = fixture;
    }

    [Fact]
    public void VCRCWithEjectorAndEconomizer_WrongRefrigerant_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithEjectorAndEconomizer(
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
                ),
                _fixture.Ejector,
                _fixture.Economizer
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
    public void VCRCWithEjectorAndEconomizer_WrongEconomizerSuperheat_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithEjectorAndEconomizer(
                _fixture.Evaporator,
                _fixture.Compressor,
                _fixture.Condenser,
                _fixture.Ejector,
                new Economizer(
                    _fixture.Economizer.TemperatureDifference,
                    TemperatureDelta.FromKelvins(50)
                )
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*Wrong temperature difference at the economizer 'hot' side!*"
            );
    }

    [Fact]
    public void VCRCWithEjectorAndEconomizer_WrongEconomizerTemperatureDifference_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithEjectorAndEconomizer(
                _fixture.Evaporator,
                _fixture.Compressor,
                _fixture.Condenser,
                _fixture.Ejector,
                new Economizer(
                    TemperatureDelta.FromKelvins(49),
                    _fixture.Economizer.Superheat
                )
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*Too high temperature difference "
                    + "at the economizer 'cold' side!*"
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
    public void Ejector_Always_ReturnsEnteredValue()
    {
        IHaveEjector sut = _fixture.Instance;
        sut.Ejector.Should().Be(_fixture.Ejector);
    }

    [Fact]
    public void Economizer_Always_ReturnsEnteredValue()
    {
        IHaveEconomizer sut = _fixture.Instance;
        sut.Economizer.Should().Be(_fixture.Economizer);
    }

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        _fixture.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void Point1_Always_ReturnsSeparatorVaporOutlet()
    {
        _fixture
            .Instance
            .Point1
            .Should()
            .Be(
                _fixture
                    .Refrigerant
                    .DewPointAt(_fixture.EjectorFlows.DiffuserOutlet.Pressure)
            );
        _fixture.Instance.Point1.Phase.Should().Be(Phases.TwoPhase);
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
                        _fixture.Instance.HeatReleaserSpecificMassFlow
                            - _fixture.Instance.IntermediateSpecificMassFlow,
                        _fixture.Instance.Point2,
                        _fixture.Instance.IntermediateSpecificMassFlow,
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
        _fixture.Instance.Point4s.Phase.Should().Be(Phases.Gas);
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
    public void Point6_Always_ReturnsEconomizerColdInlet()
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
    public void Point7_Always_ReturnsEconomizerColdOutlet()
    {
        _fixture
            .Instance
            .Point7
            .Should()
            .Be(
                _fixture
                    .Refrigerant
                    .Superheated(
                        _fixture.Instance.IntermediatePressure,
                        _fixture.Economizer.Superheat
                    )
            );
        _fixture.Instance.Point7.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point8_Always_ReturnsEconomizerHotOutlet()
    {
        _fixture
            .Instance
            .Point8
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point5
                    .CoolingTo(
                        _fixture.Instance.Point6.Temperature
                            + _fixture.Economizer.TemperatureDifference
                    )
            );
        _fixture.Instance.Point8.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point9_Always_ReturnsEjectorNozzleOutlet()
    {
        _fixture
            .Instance
            .Point9
            .Should()
            .Be(_fixture.EjectorFlows.NozzleOutlet);
        _fixture.Instance.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point10_Always_ReturnsEjectorMixingInlet()
    {
        _fixture
            .Instance
            .Point10
            .Should()
            .Be(_fixture.EjectorFlows.MixingInlet);
        _fixture.Instance.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point11_Always_ReturnsEjectorDiffuserOutlet()
    {
        _fixture
            .Instance
            .Point11
            .Should()
            .Be(_fixture.EjectorFlows.DiffuserOutlet);
        _fixture.Instance.Point11.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point12_Always_ReturnsSeparatorLiquidOutlet()
    {
        _fixture
            .Instance
            .Point12
            .Should()
            .Be(
                _fixture
                    .Refrigerant
                    .BubblePointAt(
                        _fixture.EjectorFlows.DiffuserOutlet.Pressure
                    )
            );
        _fixture.Instance.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point13_Always_ReturnsEvaporatorInlet()
    {
        _fixture
            .Instance
            .Point13
            .Should()
            .Be(
                _fixture
                    .Instance
                    .Point12
                    .IsenthalpicExpansionTo(_fixture.Evaporator.Pressure)
            );
        _fixture.Instance.Point13.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point14_Always_ReturnsEvaporatorOutlet()
    {
        _fixture.Instance.Point14.Should().Be(_fixture.Evaporator.Outlet);
        _fixture.Instance.Point14.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point15_Always_ReturnsEjectorSuctionOutlet()
    {
        _fixture
            .Instance
            .Point15
            .Should()
            .Be(_fixture.EjectorFlows.SuctionOutlet);
        _fixture.Instance.Point15.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void IntermediatePressure_ForThisCase_ReturnsMiddlePressureBetweenDiffuserOutletAndCondenserPressures() =>
        _fixture
            .Instance
            .IntermediatePressure
            .Pascals
            .Should()
            .BeApproximately(
                Math.Sqrt(
                    _fixture.EjectorFlows.DiffuserOutlet.Pressure.Pascals
                        * _fixture.Condenser.Pressure.Pascals
                ),
                50
            );

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
                    * (
                        _fixture
                            .Instance
                            .Point11
                            .Quality!
                            .Value
                            .DecimalFractions
                        / (
                            1
                            - _fixture
                                .Instance
                                .Point11
                                .Quality!
                                .Value
                                .DecimalFractions
                        )
                    )
                    * (
                        1
                        + (
                            _fixture.Instance.Point5.Enthalpy
                            - _fixture.Instance.Point8.Enthalpy
                        )
                            / (
                                _fixture.Instance.Point7.Enthalpy
                                - _fixture.Instance.Point6.Enthalpy
                            )
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
                    - _fixture.Instance.EvaporatorSpecificMassFlow
                        * (
                            _fixture
                                .Instance
                                .Point11
                                .Quality!
                                .Value
                                .DecimalFractions
                            / (
                                1
                                - _fixture
                                    .Instance
                                    .Point11
                                    .Quality!
                                    .Value
                                    .DecimalFractions
                            )
                        ),
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
                (
                    _fixture.Instance.HeatReleaserSpecificMassFlow
                    - _fixture.Instance.IntermediateSpecificMassFlow
                ).DecimalFractions
                    * (
                        _fixture.Instance.Point2s.Enthalpy
                        - _fixture.Instance.Point1.Enthalpy
                    )
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
                _fixture.Instance.Point14.Enthalpy
                    - _fixture.Instance.Point13.Enthalpy,
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
            .BeApproximately(4.783695679338165, _comparison.Tolerance);
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
            .BeApproximately(5.783610594561565, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout27() =>
        _fixture
            .AnalysisResult
            .ThermodynamicPerfection
            .Percent
            .Should()
            .BeApproximately(27.931590777519773, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout27() =>
        _fixture
            .AnalysisResult
            .MinSpecificWorkRatio
            .Percent
            .Should()
            .BeApproximately(27.8851872538937, _comparison.Tolerance);

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
            .BeApproximately(21.946588574688754, _comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        _fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        _fixture
            .AnalysisResult
            .ExpansionValvesEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(0.5128777263565665, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_ForThisCase_ReturnsAbout4() =>
        _fixture
            .AnalysisResult
            .EjectorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(4.4324100846906065, _comparison.Tolerance);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout23() =>
        _fixture
            .AnalysisResult
            .EvaporatorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(23.391508143710862, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        _fixture
            .AnalysisResult
            .RecuperatorEnergyLossRatio
            .Percent
            .Should()
            .Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        _fixture
            .AnalysisResult
            .EconomizerEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(1.7725334295847335, _comparison.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        _fixture
            .AnalysisResult
            .MixingEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(0.05889478707478723, _comparison.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        _fixture
            .AnalysisResult
            .AnalysisRelativeError
            .Percent
            .Should()
            .BeApproximately(0.166409223662599, _comparison.Tolerance);
        _fixture
            .AnalysisResult
            .Sum()
            .Percent
            .Should()
            .BeApproximately(100, _comparison.Tolerance);
    }
}
