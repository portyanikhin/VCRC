namespace VCRC.Tests;

public class VCRCWithEjectorEconomizerAndTPISubcriticalTests(
    ComparisonFixture comparison,
    SubcriticalVCRCFixture<IVCRCWithEjectorEconomizerAndTPI> fixture
)
    : IClassFixture<ComparisonFixture>,
        IClassFixture<SubcriticalVCRCFixture<IVCRCWithEjectorEconomizerAndTPI>>
{
    [Fact]
    public void VCRCWithEjectorEconomizerAndTPI_WrongRefrigerant_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithEjectorEconomizerAndTPI(
                new Evaporator(
                    FluidsList.R407C,
                    fixture.Evaporator.Temperature,
                    fixture.Evaporator.Superheat
                ),
                fixture.Compressor,
                new Condenser(
                    FluidsList.R407C,
                    fixture.Condenser.Temperature,
                    fixture.Condenser.Subcooling
                ),
                fixture.Ejector,
                fixture.Economizer
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
    public void VCRCWithEjectorEconomizerAndTPI_WrongEconomizerTemperatureDifference_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithEjectorEconomizerAndTPI(
                fixture.Evaporator,
                fixture.Compressor,
                fixture.Condenser,
                fixture.Ejector,
                new EconomizerWithTPI(TemperatureDelta.FromKelvins(49))
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
    public void Ejector_Always_ReturnsEnteredValue()
    {
        IHaveEjector sut = fixture.Instance;
        sut.Ejector.Should().Be(fixture.Ejector);
    }

    [Fact]
    public void Economizer_Always_ReturnsEnteredValue()
    {
        IHaveEconomizerWithTPI sut = fixture.Instance;
        sut.Economizer.Should().Be(fixture.Economizer);
    }

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        fixture.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void Point1_Always_ReturnsSeparatorVaporOutlet()
    {
        fixture
            .Instance.Point1.Pressure.Pascals.Should()
            .BeApproximately(
                fixture.EjectorFlows.DiffuserOutlet.Pressure.Pascals,
                50
            );
        fixture
            .Instance.Point1.Should()
            .Be(
                fixture.Refrigerant.DewPointAt(fixture.Instance.Point1.Pressure)
            );
        fixture.Instance.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point2s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance.Point2s.Should()
            .Be(
                fixture.Instance.Point1.IsentropicCompressionTo(
                    fixture.Instance.IntermediatePressure
                )
            );
        fixture.Instance.Point2s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsFirstCompressionStageDischarge()
    {
        fixture
            .Instance.Point2.Should()
            .Be(
                fixture.Instance.Point1.CompressionTo(
                    fixture.Instance.IntermediatePressure,
                    fixture.Compressor.Efficiency
                )
            );
        fixture.Instance.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point3_Always_ReturnsSecondCompressionStageSuction()
    {
        fixture
            .Instance.Point3.Should()
            .Be(
                fixture.Refrigerant.DewPointAt(
                    fixture.Instance.IntermediatePressure
                )
            );
        fixture.Instance.Point3.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point4s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance.Point4s.Should()
            .Be(
                fixture.Instance.Point3.IsentropicCompressionTo(
                    fixture.Condenser.Pressure
                )
            );
        fixture.Instance.Point4s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point4_Always_ReturnsSecondCompressionStageDischarge()
    {
        fixture
            .Instance.Point4.Should()
            .Be(
                fixture.Instance.Point3.CompressionTo(
                    fixture.Condenser.Pressure,
                    fixture.Compressor.Efficiency
                )
            );
        fixture.Instance.Point4.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point5_ForThisCase_ReturnsCondenserOutlet()
    {
        fixture.Instance.Point5.Should().Be(fixture.Condenser.Outlet);
        fixture.Instance.Point5.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point6_Always_ReturnsEconomizerColdInlet()
    {
        fixture
            .Instance.Point6.Should()
            .Be(
                fixture.Instance.Point5.IsenthalpicExpansionTo(
                    fixture.Instance.IntermediatePressure
                )
            );
        fixture.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point7_Always_ReturnsEconomizerColdOutlet()
    {
        fixture
            .Instance.Point7.Pressure.Equals(
                fixture.Instance.IntermediatePressure,
                comparison.Tolerance.Pascals()
            )
            .Should()
            .BeTrue();
        fixture
            .Instance.Point7.Should()
            .Be(
                fixture.Instance.Point6.HeatingTo(
                    (
                        (
                            fixture.Instance.Point6.Enthalpy.JoulesPerKilogram
                                * (
                                    fixture
                                        .Instance
                                        .Point2
                                        .Enthalpy
                                        .JoulesPerKilogram
                                    - fixture
                                        .Instance
                                        .Point3
                                        .Enthalpy
                                        .JoulesPerKilogram
                                )
                            + fixture.Instance.Point3.Enthalpy.JoulesPerKilogram
                                * (
                                    fixture
                                        .Instance
                                        .Point5
                                        .Enthalpy
                                        .JoulesPerKilogram
                                    - fixture
                                        .Instance
                                        .Point8
                                        .Enthalpy
                                        .JoulesPerKilogram
                                )
                        )
                        / (
                            fixture.Instance.Point2.Enthalpy.JoulesPerKilogram
                            - fixture.Instance.Point3.Enthalpy.JoulesPerKilogram
                            + fixture.Instance.Point5.Enthalpy.JoulesPerKilogram
                            - fixture.Instance.Point8.Enthalpy.JoulesPerKilogram
                        )
                    ).JoulesPerKilogram()
                )
            );
        fixture.Instance.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsEconomizerHotOutlet()
    {
        fixture
            .Instance.Point8.Should()
            .Be(
                fixture.Instance.Point5.CoolingTo(
                    fixture.Instance.Point6.Temperature
                        + fixture.Economizer.TemperatureDifference
                )
            );
        fixture.Instance.Point8.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point9_Always_ReturnsEjectorNozzleOutlet()
    {
        fixture.Instance.Point9.Should().Be(fixture.EjectorFlows.NozzleOutlet);
        fixture.Instance.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point10_Always_ReturnsEjectorMixingInlet()
    {
        fixture.Instance.Point10.Should().Be(fixture.EjectorFlows.MixingInlet);
        fixture.Instance.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point11_Always_ReturnsEjectorDiffuserOutlet()
    {
        fixture
            .Instance.Point11.Should()
            .Be(fixture.EjectorFlows.DiffuserOutlet);
        fixture.Instance.Point11.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point12_Always_ReturnsSeparatorLiquidOutlet()
    {
        fixture
            .Instance.Point12.Should()
            .Be(
                fixture.Refrigerant.BubblePointAt(
                    fixture.EjectorFlows.DiffuserOutlet.Pressure
                )
            );
        fixture.Instance.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point13_Always_ReturnsEvaporatorInlet()
    {
        fixture
            .Instance.Point13.Should()
            .Be(
                fixture.Instance.Point12.IsenthalpicExpansionTo(
                    fixture.Evaporator.Pressure
                )
            );
        fixture.Instance.Point13.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point14_Always_ReturnsEvaporatorOutlet()
    {
        fixture.Instance.Point14.Should().Be(fixture.Evaporator.Outlet);
        fixture.Instance.Point14.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point15_Always_ReturnsEjectorSuctionOutlet()
    {
        fixture
            .Instance.Point15.Should()
            .Be(fixture.EjectorFlows.SuctionOutlet);
        fixture.Instance.Point15.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void IntermediatePressure_ForThisCase_ReturnsMiddlePressureBetweenDiffuserOutletAndCondenserPressures() =>
        fixture
            .Instance.IntermediatePressure.Pascals.Should()
            .BeApproximately(
                Math.Sqrt(
                    fixture.EjectorFlows.DiffuserOutlet.Pressure.Pascals
                        * fixture.Condenser.Pressure.Pascals
                ),
                50
            );

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        fixture
            .Instance.EvaporatorSpecificMassFlow.Equals(
                100.Percent(),
                comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
        fixture
            .Instance.HeatReleaserSpecificMassFlow.Equals(
                fixture.Instance.EvaporatorSpecificMassFlow
                    * (
                        fixture.Instance.Point11.Quality!.Value.DecimalFractions
                        / (
                            1
                            - fixture
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
                            fixture.Instance.Point2.Enthalpy
                            - fixture.Instance.Point3.Enthalpy
                        )
                            / (
                                fixture.Instance.Point3.Enthalpy
                                - fixture.Instance.Point7.Enthalpy
                            )
                    ),
                comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
        fixture
            .Instance.IntermediateSpecificMassFlow.Equals(
                fixture.Instance.HeatReleaserSpecificMassFlow
                    - fixture.Instance.EvaporatorSpecificMassFlow
                        * (
                            fixture
                                .Instance
                                .Point11
                                .Quality!
                                .Value
                                .DecimalFractions
                            / (
                                1
                                - fixture
                                    .Instance
                                    .Point11
                                    .Quality!
                                    .Value
                                    .DecimalFractions
                            )
                        ),
                comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        fixture
            .Instance.IsentropicSpecificWork.Equals(
                (
                    fixture.Instance.HeatReleaserSpecificMassFlow
                    - fixture.Instance.IntermediateSpecificMassFlow
                ).DecimalFractions
                    * (
                        fixture.Instance.Point2s.Enthalpy
                        - fixture.Instance.Point1.Enthalpy
                    )
                    + fixture
                        .Instance
                        .HeatReleaserSpecificMassFlow
                        .DecimalFractions
                        * (
                            fixture.Instance.Point4s.Enthalpy
                            - fixture.Instance.Point3.Enthalpy
                        ),
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        fixture
            .Instance.SpecificWork.Equals(
                fixture.Instance.IsentropicSpecificWork
                    / fixture.Compressor.Efficiency.DecimalFractions,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        fixture
            .Instance.SpecificCoolingCapacity.Equals(
                fixture.Instance.Point14.Enthalpy
                    - fixture.Instance.Point13.Enthalpy,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        fixture
            .Instance.SpecificHeatingCapacity.Equals(
                fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                    * (
                        fixture.Instance.Point4.Enthalpy
                        - fixture.Instance.Point5.Enthalpy
                    ),
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        fixture
            .Instance.EER.Should()
            .Be(
                fixture.Instance.SpecificCoolingCapacity
                    / fixture.Instance.SpecificWork
            );
        fixture
            .Instance.EER.Should()
            .BeApproximately(4.877124735439032, comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        fixture
            .Instance.COP.Should()
            .Be(
                fixture.Instance.SpecificHeatingCapacity
                    / fixture.Instance.SpecificWork
            );
        fixture
            .Instance.COP.Should()
            .BeApproximately(5.877039217058631, comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout28() =>
        fixture
            .AnalysisResult.ThermodynamicPerfection.Percent.Should()
            .BeApproximately(28.477115061811283, comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout28() =>
        fixture
            .AnalysisResult.MinSpecificWorkRatio.Percent.Should()
            .BeApproximately(28.428456212893284, comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        fixture
            .AnalysisResult.CompressorEnergyLossRatio.Percent.Should()
            .BeApproximately(20, comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout19() =>
        fixture
            .AnalysisResult.CondenserEnergyLossRatio.Percent.Should()
            .BeApproximately(19.138658083587742, comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        fixture
            .AnalysisResult.ExpansionValvesEnergyLossRatio.Percent.Should()
            .BeApproximately(0.9728410809403587, comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_ForThisCase_ReturnsAbout4() =>
        fixture
            .AnalysisResult.EjectorEnergyLossRatio.Percent.Should()
            .BeApproximately(4.51876384630047, comparison.Tolerance);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout23() =>
        fixture
            .AnalysisResult.EvaporatorEnergyLossRatio.Percent.Should()
            .BeApproximately(23.847229676543304, comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        fixture
            .AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should()
            .Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        fixture
            .AnalysisResult.EconomizerEnergyLossRatio.Percent.Should()
            .BeApproximately(1.819717819138191, comparison.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        fixture
            .AnalysisResult.MixingEnergyLossRatio.Percent.Should()
            .BeApproximately(1.274333280596639, comparison.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        fixture
            .AnalysisResult.AnalysisRelativeError.Percent.Should()
            .BeApproximately(0.17116247380301927, comparison.Tolerance);
        fixture
            .AnalysisResult.Sum()
            .Percent.Should()
            .BeApproximately(100, comparison.Tolerance);
    }
}
