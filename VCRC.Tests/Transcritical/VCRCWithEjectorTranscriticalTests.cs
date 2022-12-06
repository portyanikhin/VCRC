namespace VCRC.Tests;

public class VCRCWithEjectorTranscriticalTests :
    IClassFixture<TranscriticalVCRCFixture<VCRCWithEjector>>
{
    public VCRCWithEjectorTranscriticalTests(
        TranscriticalVCRCFixture<VCRCWithEjector> fixture) => Fixture = fixture;

    private TranscriticalVCRCFixture<VCRCWithEjector> Fixture { get; }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Evaporator.Should().Be(Fixture.Evaporator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Compressor.Should().Be(Fixture.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsNull() =>
        Fixture.Cycle.Condenser.Should().BeNull();

    [Fact]
    public void GasCooler_ForThisCase_ReturnsEnteredValue() =>
        Fixture.Cycle.GasCooler.Should().Be(Fixture.GasCooler);

    [Fact]
    public void Ejector_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Ejector.Should().Be(Fixture.Ejector);

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsTrue() =>
        Fixture.Cycle.IsTranscritical.Should().BeTrue();

    [Fact]
    public void Point1_Always_ReturnsSeparatorVaporOutlet()
    {
        Fixture.Cycle.Point1.Should().Be(
            Fixture.Refrigerant.DewPointAt(Fixture.EjectorFlows.DiffuserOutlet.Pressure));
        Fixture.Cycle.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point2s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        Fixture.Cycle.Point2s.Should().Be(
            Fixture.Cycle.Point1.IsentropicCompressionTo(Fixture.GasCooler.Pressure));
        Fixture.Cycle.Point2s.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point2_Always_ReturnsCompressionStageDischarge()
    {
        Fixture.Cycle.Point2.Should().Be(
            Fixture.Cycle.Point1.CompressionTo(
                Fixture.GasCooler.Pressure, Fixture.Compressor.Efficiency));
        Fixture.Cycle.Point2.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point3_ForThisCase_ReturnsGasCoolerOutlet()
    {
        Fixture.Cycle.Point3.Should().Be(Fixture.GasCooler.Outlet);
        Fixture.Cycle.Point3.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point4_Always_ReturnsEjectorNozzleOutlet()
    {
        Fixture.Cycle.Point4.Should().Be(Fixture.EjectorFlows.NozzleOutlet);
        Fixture.Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point5_Always_ReturnsEjectorMixingInlet()
    {
        Fixture.Cycle.Point5.Should().Be(Fixture.EjectorFlows.MixingInlet);
        Fixture.Cycle.Point5.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point6_Always_ReturnsEjectorDiffuserOutlet()
    {
        Fixture.Cycle.Point6.Should().Be(Fixture.EjectorFlows.DiffuserOutlet);
        Fixture.Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point7_Always_ReturnsSeparatorLiquidOutlet()
    {
        Fixture.Cycle.Point7.Should().Be(
            Fixture.Refrigerant.BubblePointAt(Fixture.EjectorFlows.DiffuserOutlet.Pressure));
        Fixture.Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsEvaporatorInlet()
    {
        Fixture.Cycle.Point8.Should().Be(
            Fixture.Cycle.Point7.IsenthalpicExpansionTo(Fixture.Evaporator.Pressure));
        Fixture.Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point9_Always_ReturnsEvaporatorOutlet()
    {
        Fixture.Cycle.Point9.Should().Be(Fixture.Evaporator.Outlet);
        Fixture.Cycle.Point9.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point10_Always_ReturnsEjectorSuctionOutlet()
    {
        Fixture.Cycle.Point10.Should().Be(Fixture.EjectorFlows.SuctionOutlet);
        Fixture.Cycle.Point10.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        Fixture.Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Fixture.Cycle.HeatReleaserSpecificMassFlow.Percent.Should().BeApproximately(
            (Fixture.Cycle.EvaporatorSpecificMassFlow *
             (Fixture.Cycle.Point6.Quality!.Value.DecimalFractions /
              (1 - Fixture.Cycle.Point6.Quality!.Value.DecimalFractions))).Percent,
            Fixture.Tolerance);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        Fixture.Cycle.IsentropicSpecificWork.Should().Be(
            Fixture.Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Fixture.Cycle.Point2s.Enthalpy - Fixture.Cycle.Point1.Enthalpy));

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        Fixture.Cycle.SpecificWork.Should().Be(
            Fixture.Cycle.IsentropicSpecificWork / Fixture.Compressor.Efficiency.DecimalFractions);

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        Fixture.Cycle.SpecificCoolingCapacity.Should().Be(
            Fixture.Cycle.Point9.Enthalpy - Fixture.Cycle.Point8.Enthalpy);

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        Fixture.Cycle.SpecificHeatingCapacity.Should().Be(
            Fixture.Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Fixture.Cycle.Point2.Enthalpy - Fixture.Cycle.Point3.Enthalpy));

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        Fixture.Cycle.EER.Should().Be(
            Fixture.Cycle.SpecificCoolingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.EER.Should().BeApproximately(3.417683634972164, Fixture.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        Fixture.Cycle.COP.Should().Be(
            Fixture.Cycle.SpecificHeatingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.COP.Should().BeApproximately(4.417629862018599, Fixture.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout28() =>
        Fixture.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(19.955563041225073, Fixture.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout28() =>
        Fixture.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(19.956904468346664, Fixture.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        Fixture.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Fixture.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_Returns0() =>
        Fixture.AnalysisResult.CondenserEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_ReturnsAbout20() =>
        Fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent
            .Should().BeApproximately(20.23937422502557, Fixture.Tolerance);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout2() =>
        Fixture.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(2.7635883809857966, Fixture.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_ForThisCase_ReturnsAbout20() =>
        Fixture.AnalysisResult.EjectorEnergyLossRatio.Percent
            .Should().BeApproximately(20.506727646407636, Fixture.Tolerance);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout16() =>
        Fixture.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(16.533405279234326, Fixture.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.EconomizerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void TestMixingEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.MixingEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        Fixture.AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.006721619195616396, Fixture.Tolerance);
        Fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, Fixture.Tolerance);
    }
}