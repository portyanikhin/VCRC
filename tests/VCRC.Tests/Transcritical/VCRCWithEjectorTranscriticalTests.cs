namespace VCRC.Tests;

public class VCRCWithEjectorTranscriticalTests(
    ComparisonFixture comparison,
    TranscriticalVCRCFixture<IVCRCWithEjector> fixture
)
    : IClassFixture<ComparisonFixture>,
        IClassFixture<TranscriticalVCRCFixture<IVCRCWithEjector>>
{
    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        fixture.Instance.Evaporator.Should().Be(fixture.Evaporator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        fixture.Instance.Compressor.Should().Be(fixture.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsNull() =>
        fixture.Instance.Condenser.Should().BeNull();

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
    public void IsTranscritical_ForThisCase_ReturnsTrue() =>
        fixture.Instance.IsTranscritical.Should().BeTrue();

    [Fact]
    public void Point1_Always_ReturnsSeparatorVaporOutlet()
    {
        fixture
            .Instance
            .Point1
            .Should()
            .Be(
                fixture
                    .Refrigerant
                    .DewPointAt(fixture.EjectorFlows.DiffuserOutlet.Pressure)
            );
        fixture.Instance.Point1.Phase.Should().Be(Phases.TwoPhase);
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
                    .IsentropicCompressionTo(fixture.GasCooler.Pressure)
            );
        fixture.Instance.Point2s.Phase.Should().Be(Phases.Supercritical);
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
                        fixture.GasCooler.Pressure,
                        fixture.Compressor.Efficiency
                    )
            );
        fixture.Instance.Point2.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point3_ForThisCase_ReturnsGasCoolerOutlet()
    {
        fixture.Instance.Point3.Should().Be(fixture.GasCooler.Outlet);
        fixture.Instance.Point3.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point4_Always_ReturnsEjectorNozzleOutlet()
    {
        fixture.Instance.Point4.Should().Be(fixture.EjectorFlows.NozzleOutlet);
        fixture.Instance.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point5_Always_ReturnsEjectorMixingInlet()
    {
        fixture.Instance.Point5.Should().Be(fixture.EjectorFlows.MixingInlet);
        fixture.Instance.Point5.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point6_Always_ReturnsEjectorDiffuserOutlet()
    {
        fixture
            .Instance
            .Point6
            .Should()
            .Be(fixture.EjectorFlows.DiffuserOutlet);
        fixture.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point7_Always_ReturnsSeparatorLiquidOutlet()
    {
        fixture
            .Instance
            .Point7
            .Should()
            .Be(
                fixture
                    .Refrigerant
                    .BubblePointAt(fixture.EjectorFlows.DiffuserOutlet.Pressure)
            );
        fixture.Instance.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsEvaporatorInlet()
    {
        fixture
            .Instance
            .Point8
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point7
                    .IsenthalpicExpansionTo(fixture.Evaporator.Pressure)
            );
        fixture.Instance.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point9_Always_ReturnsEvaporatorOutlet()
    {
        fixture.Instance.Point9.Should().Be(fixture.Evaporator.Outlet);
        fixture.Instance.Point9.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point10_Always_ReturnsEjectorSuctionOutlet()
    {
        fixture
            .Instance
            .Point10
            .Should()
            .Be(fixture.EjectorFlows.SuctionOutlet);
        fixture.Instance.Point10.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        fixture
            .Instance
            .EvaporatorSpecificMassFlow
            .Equals(100.Percent(), comparison.Tolerance.Percent())
            .Should()
            .BeTrue();
        fixture
            .Instance
            .HeatReleaserSpecificMassFlow
            .Equals(
                fixture.Instance.EvaporatorSpecificMassFlow
                    * (
                        fixture.Instance.Point6.Quality!.Value.DecimalFractions
                        / (
                            1
                            - fixture
                                .Instance
                                .Point6
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
            .Instance
            .IsentropicSpecificWork
            .Equals(
                fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                    * (
                        fixture.Instance.Point2s.Enthalpy
                        - fixture.Instance.Point1.Enthalpy
                    ),
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
                fixture.Instance.Point9.Enthalpy
                    - fixture.Instance.Point8.Enthalpy,
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
                fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                    * (
                        fixture.Instance.Point2.Enthalpy
                        - fixture.Instance.Point3.Enthalpy
                    ),
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
            .BeApproximately(3.417683634972164, comparison.Tolerance);
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
            .BeApproximately(4.417629862018599, comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout28() =>
        fixture
            .AnalysisResult
            .ThermodynamicPerfection
            .Percent
            .Should()
            .BeApproximately(19.955563041225073, comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout28() =>
        fixture
            .AnalysisResult
            .MinSpecificWorkRatio
            .Percent
            .Should()
            .BeApproximately(19.956904468346664, comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        fixture
            .AnalysisResult
            .CompressorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(20, comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_Returns0() =>
        fixture.AnalysisResult.CondenserEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_ReturnsAbout20() =>
        fixture
            .AnalysisResult
            .GasCoolerEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(20.23937422502557, comparison.Tolerance);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout2() =>
        fixture
            .AnalysisResult
            .ExpansionValvesEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(2.7635883809857966, comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_ForThisCase_ReturnsAbout20() =>
        fixture
            .AnalysisResult
            .EjectorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(20.506727646407636, comparison.Tolerance);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout16() =>
        fixture
            .AnalysisResult
            .EvaporatorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(16.533405279234326, comparison.Tolerance);

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
            .BeApproximately(0.006721619195616396, comparison.Tolerance);
        fixture
            .AnalysisResult
            .Sum()
            .Percent
            .Should()
            .BeApproximately(100, comparison.Tolerance);
    }
}
