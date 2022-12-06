namespace VCRC.Tests;

public class SimpleVCRCTranscriticalTests :
    IClassFixture<TranscriticalVCRCFixture<SimpleVCRC>>
{
    public SimpleVCRCTranscriticalTests(
        TranscriticalVCRCFixture<SimpleVCRC> fixture) => Fixture = fixture;

    private TranscriticalVCRCFixture<SimpleVCRC> Fixture { get; }

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
    public void IsTranscritical_ForThisCase_ReturnsTrue() =>
        Fixture.Cycle.IsTranscritical.Should().BeTrue();

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        Fixture.Cycle.Point1.Should().Be(Fixture.Evaporator.Outlet);
        Fixture.Cycle.Point1.Phase.Should().Be(Phases.Gas);
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
    public void Point4_Always_ReturnsEvaporatorInlet()
    {
        Fixture.Cycle.Point4.Should().Be(
            Fixture.Cycle.Point3.IsenthalpicExpansionTo(Fixture.Evaporator.Pressure));
        Fixture.Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        Fixture.Cycle.SpecificWork.Should().Be(
            Fixture.Cycle.IsentropicSpecificWork / Fixture.Compressor.Efficiency.DecimalFractions);

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        Fixture.Cycle.SpecificCoolingCapacity.Should().Be(
            Fixture.Cycle.Point1.Enthalpy - Fixture.Cycle.Point4.Enthalpy);

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        Fixture.Cycle.SpecificHeatingCapacity.Should().Be(
            Fixture.Cycle.Point2.Enthalpy - Fixture.Cycle.Point3.Enthalpy);

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        Fixture.Cycle.EER.Should().Be(
            Fixture.Cycle.SpecificCoolingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.EER.Should().BeApproximately(2.6245123507309613, Fixture.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        Fixture.Cycle.COP.Should().Be(
            Fixture.Cycle.SpecificHeatingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.COP.Should().BeApproximately(3.624512350730961, Fixture.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout15() =>
        Fixture.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(15.32430361065648, Fixture.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout15() =>
        Fixture.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(15.32430361065648, Fixture.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        Fixture.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Fixture.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_Returns0() =>
        Fixture.AnalysisResult.CondenserEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        Fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent
            .Should().BeApproximately(21.94242006423027, Fixture.Tolerance);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout30() =>
        Fixture.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(30.204839020585684, Fixture.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout12() =>
        Fixture.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(12.52843730452757, Fixture.Tolerance);

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
            .Should().BeApproximately(0, Fixture.Tolerance);
        Fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, Fixture.Tolerance);
    }
}