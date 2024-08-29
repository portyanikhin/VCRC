namespace VCRC.Tests;

public sealed class VCRCWithEjectorAndEconomizerTranscriticalTests(
    ComparisonFixture comparison,
    TranscriticalVCRCFixture<IVCRCWithEjectorAndEconomizer> fixture
)
    : IClassFixture<ComparisonFixture>,
        IClassFixture<TranscriticalVCRCFixture<IVCRCWithEjectorAndEconomizer>>
{
    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        fixture.Instance.Evaporator.Should().Be(fixture.Evaporator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        fixture.Instance.Compressor.Should().Be(fixture.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsNull() => fixture.Instance.Condenser.Should().BeNull();

    [Fact]
    public void GasCooler_ForThisCase_ReturnsEnteredValue() =>
        fixture.Instance.GasCooler.Should().Be(fixture.GasCooler);

    [Fact]
    public void Ejector_Always_ReturnsEnteredValue()
    {
        IHaveEjector sut = fixture.Instance;
        sut.Ejector.Should().Be(fixture.Ejector);
    }

    [Fact]
    public void Economizer_Always_ReturnsEnteredValue()
    {
        IHaveEconomizer sut = fixture.Instance;
        sut.Economizer.Should().Be(fixture.Economizer);
    }

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsTrue() =>
        fixture.Instance.IsTranscritical.Should().BeTrue();

    [Fact]
    public void Point1_Always_ReturnsSeparatorVaporOutlet()
    {
        fixture
            .Instance.Point1.Should()
            .Be(fixture.Refrigerant.DewPointAt(fixture.EjectorFlows.DiffuserOutlet.Pressure));
        fixture.Instance.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point2s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance.Point2s.Should()
            .Be(
                fixture.Instance.Point1.IsentropicCompressionTo(
                    fixture.Instance.IntermediatePressure
                )
            );
        fixture.Instance.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
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
        fixture.Instance.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_Always_ReturnsSecondCompressionStageSuction()
    {
        fixture
            .Instance.Point3.Should()
            .Be(
                fixture.Refrigerant.Mixing(
                    fixture.Instance.HeatReleaserSpecificMassFlow
                        - fixture.Instance.IntermediateSpecificMassFlow,
                    fixture.Instance.Point2,
                    fixture.Instance.IntermediateSpecificMassFlow,
                    fixture.Instance.Point7
                )
            );
        fixture.Instance.Point3.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point4s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance.Point4s.Should()
            .Be(fixture.Instance.Point3.IsentropicCompressionTo(fixture.GasCooler.Pressure));
        fixture.Instance.Point4s.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point4_Always_ReturnsSecondCompressionStageDischarge()
    {
        fixture
            .Instance.Point4.Should()
            .Be(
                fixture.Instance.Point3.CompressionTo(
                    fixture.GasCooler.Pressure,
                    fixture.Compressor.Efficiency
                )
            );
        fixture.Instance.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point5_ForThisCase_ReturnsGasCoolerOutlet()
    {
        fixture.Instance.Point5.Should().Be(fixture.GasCooler.Outlet);
        fixture.Instance.Point5.Phase.Should().Be(Phases.Supercritical);
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
            .Instance.Point7.Should()
            .Be(
                fixture.Refrigerant.Superheated(
                    fixture.Instance.IntermediatePressure,
                    fixture.Economizer.Superheat
                )
            );
        fixture.Instance.Point7.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point8_Always_ReturnsEconomizerHotOutlet()
    {
        fixture
            .Instance.Point8.Should()
            .Be(
                fixture.Instance.Point5.CoolingTo(
                    fixture.Instance.Point6.Temperature + fixture.Economizer.TemperatureDifference
                )
            );
        fixture.Instance.Point8.Phase.Should().Be(Phases.Supercritical);
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
        fixture.Instance.Point11.Should().Be(fixture.EjectorFlows.DiffuserOutlet);
        fixture.Instance.Point11.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point12_Always_ReturnsSeparatorLiquidOutlet()
    {
        fixture
            .Instance.Point12.Should()
            .Be(fixture.Refrigerant.BubblePointAt(fixture.EjectorFlows.DiffuserOutlet.Pressure));
        fixture.Instance.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point13_Always_ReturnsEvaporatorInlet()
    {
        fixture
            .Instance.Point13.Should()
            .Be(fixture.Instance.Point12.IsenthalpicExpansionTo(fixture.Evaporator.Pressure));
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
        fixture.Instance.Point15.Should().Be(fixture.EjectorFlows.SuctionOutlet);
        fixture.Instance.Point15.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void IntermediatePressure_ForThisCase_ReturnsMiddlePressureBetweenDiffuserOutletAndGasCoolerPressures() =>
        fixture
            .Instance.IntermediatePressure.Pascals.Should()
            .BeApproximately(
                Math.Sqrt(
                    fixture.EjectorFlows.DiffuserOutlet.Pressure.Pascals
                        * fixture.GasCooler.Pressure.Pascals
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
                        / (1 - fixture.Instance.Point11.Quality!.Value.DecimalFractions)
                    )
                    * (
                        1
                        + (fixture.Instance.Point5.Enthalpy - fixture.Instance.Point8.Enthalpy)
                            / (fixture.Instance.Point7.Enthalpy - fixture.Instance.Point6.Enthalpy)
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
                            fixture.Instance.Point11.Quality!.Value.DecimalFractions
                            / (1 - fixture.Instance.Point11.Quality!.Value.DecimalFractions)
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
                    * (fixture.Instance.Point2s.Enthalpy - fixture.Instance.Point1.Enthalpy)
                    + fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                        * (fixture.Instance.Point4s.Enthalpy - fixture.Instance.Point3.Enthalpy),
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
                fixture.Instance.Point14.Enthalpy - fixture.Instance.Point13.Enthalpy,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        fixture
            .Instance.SpecificHeatingCapacity.Equals(
                fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                    * (fixture.Instance.Point4.Enthalpy - fixture.Instance.Point5.Enthalpy),
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        fixture
            .Instance.EER.Should()
            .Be(fixture.Instance.SpecificCoolingCapacity / fixture.Instance.SpecificWork);
        fixture.Instance.EER.Should().BeApproximately(3.5076990300399533, comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        fixture
            .Instance.COP.Should()
            .Be(fixture.Instance.SpecificHeatingCapacity / fixture.Instance.SpecificWork);
        fixture.Instance.COP.Should().BeApproximately(4.507637142530747, comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout20() =>
        fixture
            .AnalysisResult.ThermodynamicPerfection.Percent.Should()
            .BeApproximately(20.48115525010449, comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout20() =>
        fixture
            .AnalysisResult.MinSpecificWorkRatio.Percent.Should()
            .BeApproximately(20.44763406769744, comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        fixture
            .AnalysisResult.CompressorEnergyLossRatio.Percent.Should()
            .BeApproximately(20, comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_Returns0() =>
        fixture.AnalysisResult.CondenserEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        fixture
            .AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should()
            .BeApproximately(21.62366337364647, comparison.Tolerance);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        fixture
            .AnalysisResult.ExpansionValvesEnergyLossRatio.Percent.Should()
            .BeApproximately(6.0218512390103545, comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_ForThisCase_ReturnsAbout12() =>
        fixture
            .AnalysisResult.EjectorEnergyLossRatio.Percent.Should()
            .BeApproximately(12.556916295285012, comparison.Tolerance);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout16() =>
        fixture
            .AnalysisResult.EvaporatorEnergyLossRatio.Percent.Should()
            .BeApproximately(16.953384710916637, comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout2() =>
        fixture
            .AnalysisResult.EconomizerEnergyLossRatio.Percent.Should()
            .BeApproximately(2.199955238320946, comparison.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        fixture
            .AnalysisResult.MixingEnergyLossRatio.Percent.Should()
            .BeApproximately(0.19659507512313654, comparison.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        fixture
            .AnalysisResult.AnalysisRelativeError.Percent.Should()
            .BeApproximately(0.16393672879741025, comparison.Tolerance);
        fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, comparison.Tolerance);
    }
}
